# Cyber battle Online

## Author

Rafael José, a22202078

## Description

Cyber Battle is a top-down shooter game where the objective for players is to achieve the highest score possible within the time limit. It is a multiplayer game that uses Unity Networking (NGO) to synchronize player actions. Players compete against each other, collecting score-giving points scattered across the map, while the network ensures all interactions are recorded and reflected in real-time for all participants. When a player is eliminated, their points are reset.

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

