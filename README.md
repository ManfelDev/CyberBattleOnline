# Cyber Battle Online

## Author

Rafael José, a22202078

## Description

Cyber Battle is a top-down shooter game, with laser guns, where the objective for players is to achieve the highest score possible within the time limit. It is a multiplayer game that uses Unity Networking (NGO) and Unity Relay to synchronize player actions and facilitate connections. Players compete against each other, collecting score-giving points scattered across the map, while the network ensures all interactions are recorded and reflected in real-time for all participants. When a player is eliminated, their points are reset.

![Gameplay](./Images/gameplay.png)

## Initialization

When the executable is launched, the user initially has the option to start a server or a client. If the user clicks the "Start Server" button, the ```OnClickServer()``` method is called. In this method, the ```UnityTransport``` is configured, and it checks if the selected protocol is Relay. If it is, ```isRelay``` is set to true and the code input interface is displayed. Next, the ```StartCoroutine(StartAsServerCR())``` method is called to initiate the server process.

Within the ```StartAsServerCR()``` coroutine, the ```NetworkManager``` and ```UnityTransport``` are enabled, and callbacks for client connection and disconnection are registered. If the transport is Relay, the server performs anonymous login to Unity services, creates an allocation for the session, and obtains a join code. This code is displayed on the server interface for other players to use to connect. The server is then started with ```networkManager.StartServer()``` and begins accepting client connections.

If the user clicks the "Start Client" button, the ```OnClickClient()``` method is called. Similar to the server initialization, the ```UnityTransport``` is configured, and it checks if the selected protocol is Relay. If it is, ```isRelay``` is set to ```true```, and the interface to enter a join code and the player's nickname is displayed. The player can then enter the join code provided by the server along with their nickname and click to connect.

The ```StartAsClientCR()``` coroutine manages the client's connection process. After enabling the ```NetworkManager``` and ```UnityTransport```, the method performs anonymous login to Unity services. Using the join code, the client obtains the allocation details needed to configure the transport and connect to the server. The client then initiates the connection with ```networkManager.StartClient()```, and if the connection is successful, the user interface is updated to display the game state. If the join code is invalid, a message is displayed indicating the invalid code.

![Initialization](./Images/initialization.png)

## Player

In NGO, the server controls everything (Server authority). All the clients do is view the game and provide inputs, which the server processes to produce the output.
The main disadvantage of this approach is that whenever the player wants to do something, they have to send a message to the server, which decides and sends back a message with what will happen. This introduces latency based on the player's connection to the server, resulting in the total latency being the player's latency to the server plus the latency of receiving the server's message. For instance, if this latency is 10ms, the client's latency would be 10ms + 10ms = 20ms, resulting in a 20-millisecond delay between the action and what the player perceives will happen.

![ping-animation-dark](https://github.com/ManfelDev/CyberBattleOnline/assets/115217461/ccbb6d8b-2ffd-4544-917c-d79110380fb7)

### Movement

If the player’s movement is linked to server-authoritative code, every time the player presses to move, there will always be a delay before the action actually happens. To avoid this, I gave authority to the clients over their own controls, making the game more responsive. However, this brings the risk of someone hacking the clients and teleporting around the map since the server will trust the information sent by the clients.

To implement this, I used Unity’s ```NetworkTransform``` and created a ```ClientNetworkTransform```. I overrode the ```OnIsServerAuthoritative()``` method to return false, disabling server authority. This means the client can directly update the object’s transform. On the client side, we need to identify if we own the object to update its position. To achieve this, I overrode the ```OnNetworkSpawn()``` method so that when players are spawned, they are set as owners. If it is our player, we can assume the transform, updating its position and rotation. I also overrode the ```Update()``` method, passing the information every frame that we continue to own the object, along with performing security checks to ensure the ```NetworkManager``` is connected to a client or is listening. If these conditions are true, we can alter the transform on the server, passing our transform and the time since the last synchronization using the ```NetworkManager```'s local time. This is necessary because each frame’s duration varies, and we need to interpolate the positions for smooth movement on the network.

For player movement, as well as for other objects that need initialization, instead of using the ```Start()``` method, which starts too early, I used ```OnNetworkSpawn()```. In networking, there can be some delay, and some things need to be set up first. ```OnNetworkSpawn()``` functions much like the ```Start()``` method but is called when everything on the network is already set up.

When synchronized, ```OnNetworkSpawn()``` can define the owner of the object and all relevant information about the object. In this case, I checked if the client owns the player to execute the following code only if they are the owner. I applied the same approach in the ```Update()```, ```FixedUpdate()```, and ```LateUpdate()``` methods.

### Health

For the health system, I used a network variable (```NetworkVariable<int>```) to ensure that all clients are aware of changes in health and can update the UI accordingly. This variable can only be modified by the server, and if a client tries to alter it, nothing happens. Each player's health is managed exclusively by the server and synchronized with the clients. When the object is instantiated on the network (```OnNetworkSpawn()```), the health is initialized by the server. If the player takes damage, the ```TakeDamage()``` method is called, which modifies the player's health through the ```ModifyHealth()``` method. This method ensures that the player's health does not exceed the defined limits, and if health reaches zero, it triggers an event to indicate that the player has died. Similarly, if the player heals, the Heal() method is called, which also uses ```ModifyHealth()``` to adjust the player's health within the allowed limits.

The health UI is updated across all clients through listeners added to the network variable ```CurrentHealth```. These listeners ensure that the health bar and color are updated whenever the player's health changes.

![Player Health](./Images/player_health.png)

### Score

For the score, I again used a network variable (```NetworkVariable<int>```) so that all clients are aware of score changes and can update the user interface (UI) accordingly. This network variable can only be modified by the server; if a client attempts to change it, nothing happens. Each player's score is managed exclusively by the server and then synchronized with the clients. When a player earns points, the ```AddScore()``` method is called, incrementing the ```Score``` variable. This update is automatically propagated to all clients. 

The score UI is updated through the ```ScoreDisplay``` script, which periodically checks if the local player is known and, if so, updates the score text on the screen. This script ensures that the displayed score is always that of the player/client. Each client only sees their own updated score.

![Player Score](./Images/player_score.png)

### Respawn/despawn

For the respawn and despawn of players, I used a respawn management system that ensures players are recreated after dying. The ```RespawnManager``` script is responsible for managing these events. When a player is instantiated on the network (```OnNetworkSpawn()```), the script checks if it is the server and, if so, subscribes to the player spawn and despawn events. The ```OnPlayerSpawned``` event is used to associate each player's death event (```OnDie```) with a method that handles respawning. When a player dies, the ```PlayerDie()``` method is called, which destroys the player object and starts a coroutine (```RespawnPlayer```) to recreate the player after a defined wait time (```respawnDelay```).

The ```RespawnPlayer``` coroutine waits for the defined respawn time and then obtains a new spawn position from the ```SpawnManager```. This manager selects an appropriate position for the player, ensuring that they do not spawn too close to other players. A new player object is then instantiated and configured for the original client. The ```SpawnManager``` provides the spawn positions and checks player proximity to prevent collisions during respawn.

### Names Above the Head

For the player names, I used a combination of ```ServerRpc``` and ```ClientRpc``` to ensure all clients have access to player names and can update the UI accordingly. When a player enters the game, the ```SubmitPlayerNameServerRpc()``` method sends the player's name to the server. The client then requests all player names using ```RequestAllPlayerNamesServerRpc()```.

When the network object is instantiated (```OnNetworkSpawn()```), if it is the local player, the name input from the player from ```JoinManager``` is sent to the server via ```SubmitPlayerNameServerRpc()```, and all player names are requested with ```RequestAllPlayerNamesServerRpc()```, the server updates the name information with that and uses ```UpdatePlayerNameClientRpc()``` to notify all clients about the player's name and to ensure the client knows the names of all other players. The ```OnPlayerNameChanged``` event is triggered to notify listeners the new player added, so that they can also know the player's name.

![Player Name](./Images/player_name.png)

## Camera

The camera smoothly follows the local player by initializing with the ```FindLocalPlayer()``` method, which searches among all players and returns the transform of the player that belongs to the local client.

In the ```Start()``` method, the camera locates the local player. During each frame in the ```Update()``` method, the camera checks again if the local player is defined. If not, it attempts to locate the player again using ```FindLocalPlayer()```.

Once the local player is found, the camera calculates the desired position based on the player's current position, keeping the z-coordinate constant to maintain the camera's depth. The camera's position is then smoothed using ```Vector3.Lerp```, interpolating the camera's current position to the desired position based on the smoothing speed (```smoothSpeed```).

![camera](https://github.com/ManfelDev/CyberBattleOnline/assets/115217461/1699486f-007a-4e9e-921a-7b5f763f981c)

## Projectile

To create projectiles, specifically the lasers fired by players, I used the following method:

- **Spawn of a "dummy projectile":** When the player clicks to shoot, a "dummy" projectile is created before anything is sent over the network. This "dummy" projectile is purely visual and does not cause any damage, making the game feel more responsive. (```SpawnDummyProjectile()```)
- **Sending an RPC:** After creating the "dummy" projectile, an RPC (Remote Procedure Call) is sent to the server to attempt firing the actual projectile. An RPC is a remote procedure call where the player sends a message to the server. (```PrimaryFireServerRpc()```)
- **Validation and Spawn on the Server:** The server receives the RPC, validates whether the player meets the necessary conditions to shoot, and if everything is correct, creates the real projectile. This real projectile, being on the server side, does not need any visuals. (```PrimaryFireServerRpc()```)
- **Broadcast to Clients:** The server sends an RPC to all clients, requesting them to create a "dummy projectile." The original client that fired the projectile can ignore this RPC. (```SpawnDummyProjectileClientRpc()```)

This method has several benefits, such as allowing the player to see a projectile immediately upon shooting, making the game feel more responsive. Additionally, it maintains server authority, ensuring that a player can never cause damage to someone without the server's validation. The server handles the damage and all important aspects of the projectile.

![Projectiles](./Images/projectiles.png)

*(Screenshot of the same frame of a shot fired from the server side and the client side (a sprite renderer was added to the real projectile just to take the screenshot))*

## Leaderboard

For the leaderboard, I used a ```NetworkList``` to maintain the list of players and their scores synchronized between the server and all clients. When a player enters the game, their data is added to the ```NetworkList<LeaderboardEntityState>```, ```LeaderboardEntityState``` is a struct created to store necessary information from the clients, which is monitored by all clients to update the leaderboard visually.

When the network object is instantiated (```OnNetworkSpawn()```), the leaderboard is configured so that clients monitor changes in the list (```OnLeaderboardEntitiesChanged()```). The ```OnLeaderboardEntitiesChanged()``` method uses a switch statement to handle different types of changes to the ```NetworkList```, such as adding, removing, or updating players. When adding a player to the leaderboard, the server creates a new instance of ```LeaderboardEntityState``` with the client's ID, the player's name, and the initial score. This state is then added to the ```NetworkList```, which notifies all clients to update their leaderboard displays.

The ```OnPlayerSpawned``` and ```OnPlayerDespawned``` events are used to add and remove players from the leaderboard. When a player spawns, the server initializes a ```LeaderboardEntityState``` for that player and adds it to the ```NetworkList```. Conversely, when a player despawns, their entry is removed from the list.

The ```ClientRpc``` method ```UpdatePlayerNameClientRpc()``` ensures that players' names are propagated to all clients when the player joins the game. The players' scores and their ranking on the leaderboard are automatically updated through the ```NetworkList```, and these changes are reflected in real time in the UI. The leaderboard is sorted by score, from highest to lowest, ensuring that the players with the best scores appear at the top.

The leaderboard UI is managed by the ```LeaderBoardEntityDisplay``` script, which visually updates the players' positions and scores. Additionally, the local player's name is displayed in a different color to highlight their position if they are on the leaderboard. This visual differentiation helps players easily identify their rank among other players.

![Leaderboard](./Images/leaderboard.png)

## Bonus Score

To implement the bonus scores, the server controls the creation and management of these bonuses. The bonuses are network objects (```NetworkObject```) instantiated by the server in random positions within a defined area (```spawnArea```) in the ```OnNetworkSpawn()``` method. These positions are checked to avoid unwanted areas (```avoidLayers```). The score for each bonus is set using the ```SetScore()``` method.

When a player attempts to collect a bonus, the ```Pick()``` method is called. This method checks on the server whether the bonus has already been collected. To avoid lag effects, the bonus immediately disappears on the client side. The server then validates the collection: if the bonus was successfully collected, the server updates the player's score and sends the confirmation back to the client; if not, the server informs the client that the collection failed, and the bonus respawns.

The bonus collection is protected to ensure that two players do not collect the same bonus simultaneously. The server is the final authority that determines who collects the bonus first. When a bonus is collected, the ```OnPicked``` event is triggered, activating the ```OnBonusScorePicked()``` method on the server. This method repositions the bonus to a new random location and prepares it to be collected again by calling the ```Respawn()``` method, synchronizing these changes with all connected clients.

![Bonus Score](./Images/bonus_score.png)

## Healing Spaces

Healing spaces are managed by the server, which controls their activation and deactivation. When a player enters the area of a healing space, the server checks if the player has less than the maximum health. If so, the player is healed by the amount defined in ```healAmount```, and the healing space is deactivated, starting a cooldown through the ```NetworkVariable``` ```remainingHealCooldown```.

To avoid lag effects, the healing space is immediately deactivated on the client side, and the server validates the healing, updating the ```isActive``` variable to synchronize the deactivation with all clients. The cooldown timer is decremented by the server in the ```Update()``` method, and when it reaches zero, the healing space is reactivated, updating the ```isActive``` variable again.

Changes in the ```isActive``` and ```remainingHealCooldown``` variables trigger the ```OnActiveChanged()``` and ```OnCooldownChanged()``` methods, which visually update the models and the timer on the clients.

![Healing Spaces](./Images/healing_spaces.png)

## Game Manager

The ```GameManager``` controls the start, end, and restart of games, managing the game timer, player scores, and overall game state. In the ```OnNetworkSpawn()``` method, if the server is active, the game starts by calling the ```StartGame()``` method, which sets the game duration and begins the countdown with the ```GameTimerCoroutine()```.

The game timer is managed by the server, which decrements the value every second and updates all clients through the ```UpdateTimerClientRpc()``` method, keeping the time synchronized across all devices. When the timer reaches zero, the game ends, and the ```EndGame()``` method is called. This method disables player movement and shooting and removes active projectiles in the game through ```EndGameClientRpc()``` and ```EndGameServerRpc()```, ensuring all actions stop until a new game begins. The ```EndGameServerRpc()``` method removes projectiles that cause real damage, while the ```EndGameClientRpc()``` method removes visual projectiles (dummy projectiles).

After the game ends, the coroutine ```NewGameCountdownCoroutine()``` starts a countdown for the next game, updating clients with the current winner and the time remaining until the next game through ```UpdateEndGameTextClientRpc()```. When the countdown ends, the ```RestartGame()``` method is called, resetting the health and scores of players, resetting healing spaces, and reactivating player movement and shooting. The initial positions of players are also reset using a new random spawn, and these actions are synchronized with clients using ```StartNewGameClientRpc()```.

![game_manager](https://github.com/ManfelDev/CyberBattleOnline/assets/115217461/3749d638-81ef-4dd3-85b8-d3e0c547bfaf)

## Network architecture diagram

Unity Relay is a service that facilitates multiplayer connectivity by routing traffic between players' devices without requiring a dedicated server. Relay helps to establish connections when direct peer-to-peer communication is not possible due to network configurations such as NATs or firewalls. By acting as an intermediary, Relay ensures that all players can connect to each other reliably. The Unity Relay system enhances the security of the peer-to-peer model by adding a server that acts as an intermediary for the connection between the Host and Clients.

Advantages of using Unity Relay include easier setup for multiplayer games, reduced need for complex network configurations, and improved connectivity in cases where direct communication is not feasible. However, there are also some disadvantages, such as potential latency introduced by the relay server and the added cost associated with using the Relay service. Additionally, the reliance on an external service introduces a dependency that could affect game performance if the service experiences downtime or other issues.

Below is a technical example of how this connection work, illustrating the use of Unity Relay with a the maximum players connected, of 10 players.

![Network Diagram](./Images/network_diagram.png)

## Bandwidth

In this section, I will be showing the bandwidth usage for each of the following actions:

### Client Request Connection

When a client requests a connection to the server:

![Bandwidth Client Request Connection](./Images/bandwidth_connection_request.png)

### Client Connection

When a client connects to the server:

![Bandwidth Player Connection](./Images/bandwidth_connection.png)

### Ownership and player spawn

When a player spawns and takes ownership of their object:

![Bandwidth Ownership and Player Spawn - NGO Messages](./Images/bandwidth_ownership.png)

### Enter player on the leaderboard

When a player enters the game and is added to the leaderboard:

![Bandwidth Enter Player on the Leaderboard](./Images/bandwidth_leaderboard.png)

### Load leaderboard entities

When the leaderboard is loaded:

![Bandwidth Load Leaderboard Entities](./Images/bandwidth_leaderboard_entities.png)

### Player movement

When a player moves (in this case, the player is moving up and to the right, while rotating, a mix of movement and rotation):

![Bandwidth Player Movement](./Images/bandwidth_player_movement.png)

### Player shooting

When a player shoots, creating a real projectile:

![Bandwidth Player Shooting - Real Projectile](./Images/bandwidth_real_projectile.png)

When a player shoots, creating a dummy projectile for all the clients:

![Bandwidth Player Shooting - Dummy Projectile](./Images/bandwidth_dummy_projectile.png)

### Player health

When a player's health is updated:

![Bandwidth Player Health](./Images/bandwidth_player_health.png)

### Player die

When a player dies / despawns:

![Bandwidth Player Die](./Images/bandwidth_player_die.png)

### Using a healing space

When a player uses a healing space:

![Bandwidth Healing Space](./Images/bandwidth_healingspace_use.png)

### Healing Space cooldown

When a healing space is on cooldown:

![Bandwidth Healing Space Cooldown](./Images/bandwidth_healingspace_cooldown.png)

### Healing Space reactivation

When a healing space is reactivated:

![Bandwidth Healing Space Reactivation](./Images/bandwidth_healingspace_reactivation.png)

### Bonus score

When a player collects a bonus score:

![Bandwidth Bonus Score](./Images/bandwidth_bonus_score.png)

### Game timer

When the game timer is updated:

![Bandwidth Game Timer](./Images/bandwidth_game_timer.png)

### End game

When the game ends and the end game countdown starts, showing the winner and the time remaining until the next game starts:

![Bandwidth End Game](./Images/bandwidth_end_game_countdown.png)

When the end game countdown ends and a new game starts:

### New game

When the game manager starts a new game:

![Bandwidth New Game](./Images/bandwidth_new_game.png)

## References

- [Unity - Network for GameObjects API](https://docs-multiplayer.unity3d.com/netcode/1.6.0/about/)
- [Unity - Latency and Packet Loss](https://docs-multiplayer.unity3d.com/netcode/1.6.0/learn/lagandpacketloss/)
- [Unity - Relay](https://docs.unity.com/ugs/en-us/manual/relay/manual/get-started)
- [Unity - Network Profiler](https://docs-multiplayer.unity3d.com/tools/current/profiler/index.html)
- [Sistemas de Redes para Jogos - Aula 15/05/2024](https://www.youtube.com/watch?v=y7ETO57_kQY)
- [Sistemas de Redes para Jogos - Aula 22/05/2024](https://www.youtube.com/watch?v=NWwIrN_hJwU)
- [Sistemas de Redes para Jogos - Aula 29/05/2024](https://www.youtube.com/watch?v=FNntUfrpwWI)
- [COMPLETE Unity Multiplayer Tutorial (Netcode for Game Objects)](https://www.youtube.com/watch?v=3yuBOB3VrCk)
- [Top-down Tanks Redux · Kenney - For the background and walls](https://kenney.nl/assets/top-down-tanks-redux)
