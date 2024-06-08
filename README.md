# Cyber Battle Online

## Author

Rafael José, a22202078

## Description

Cyber Battle is a top-down shooter game, with laser guns, where the objective for players is to achieve the highest score possible within the time limit. It is a multiplayer game that uses Unity Networking (NGO) to synchronize player actions. Players compete against each other, collecting score-giving points scattered across the map, while the network ensures all interactions are recorded and reflected in real-time for all participants. When a player is eliminated, their points are reset.

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

For the player names, I used a combination of  ```ServerRpc``` and ```ClientRpc``` to ensure that all clients have access to the player's name and can update the user interface (UI) accordingly. When a player enters the game, the ```SubmitPlayerNameServerRpc()``` method is called, sending the player's name to the server. The server, in turn, propagates this information to all clients using the ```UpdatePlayerNameClientRpc()``` method.

When the network object is instantiated (```OnNetworkSpawn()```), the player's name is configured and synchronized. If it is the local player, the name is obtained from the ```JoinManager```, which is where the client enters their nickname at the beginning of the application, and sent to the server via ```SubmitPlayerNameServerRpc()```. The server updates the name information and uses ```UpdatePlayerNameClientRpc()``` to notify all clients about the player's name. The ```OnPlayerNameChanged``` event is executed to notify any listeners about the name setting.

Additionally, when requesting all player names (```RequestAllPlayerNamesServerRpc()```), the server sends the names of all connected players to the client that made the request, ensuring that the player list is always up to date. This system ensures that player names are synchronized. In the game, the player's name can only be set at the beginning and cannot be changed later.

![Player Name](./Images/player_name.png)

## Camera

The camera smoothly follows the local player by initializing with the ```FindLocalPlayer()``` method, which searches among all players and returns the transform of the player that belongs to the local client.

In the ```Start()``` method, the camera locates the local player. During each frame in the ```Update()``` method, the camera checks again if the local player is defined. If not, it attempts to locate the player again using ```FindLocalPlayer()```.

Once the local player is found, the camera calculates the desired position based on the player's current position, keeping the z-coordinate constant to maintain the camera's depth. The camera's position is then smoothed using ```Vector3.Lerp```, interpolating the camera's current position to the desired position based on the smoothing speed (```smoothSpeed```).

## Projectile

To create projectiles, specifically the lasers fired by players, I used the following method:

- **Spawn of a "dummy projectile":** When the player clicks to shoot, a "dummy" projectile is created before anything is sent over the network. This "dummy" projectile is purely visual and does not cause any damage, making the game feel more responsive. (```SpawnDummyProjectile()```)
- **Sending an RPC:** After creating the "dummy" projectile, an RPC (Remote Procedure Call) is sent to the server to attempt firing the actual projectile. An RPC is a remote procedure call where the player sends a message to the server. (```PrimaryFireServerRpc()```)
- **Validation and Spawn on the Server:** The server receives the RPC, validates whether the player meets the necessary conditions to shoot, and if everything is correct, creates the real projectile. This real projectile, being on the server side, does not need any visuals. (```PrimaryFireServerRpc()```)
- **Broadcast to Clients:** The server sends an RPC to all clients, requesting them to create a "dummy projectile." The original client that fired the projectile can ignore this RPC. (```SpawnDummyProjectileClientRpc()```)

This method has several benefits, such as allowing the player to see a projectile immediately upon shooting, making the game feel more responsive. Additionally, it maintains server authority, ensuring that a player can never cause damage to someone without the server's validation. The server handles the damage and all important aspects of the projectile.

![Projectiles](./Images/projectiles.png)

*(Print screen of the same frame of a shot fired from the server side and the client side (a sprite renderer was added to the real projectile just to take the print screen))*
