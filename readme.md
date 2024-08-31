# Quoridor - Unity Multiplayer Turn-Based Game

Welcome to the **Quoridor** project! This repository contains the source code for a Unity-based, online multiplayer version of the classic board game Quoridor. The project was developed as part of a summer course in Unity, focusing on implementing advanced game mechanics, AI algorithms, and networking features.

## Getting Started

### Play Online

[Play Quoridor Online](https://quoridor-webgl.netlify.app/)
- **NO MULTIPLAYER** (will be fixed in the future)
   
### Download

You can download the latest version of Quoridor for various platforms from the links below:

- **[Download Quoridor for PC](https://github.com/whyt1/Quoridor/releases/download/PC_Build/Quoridor._PC.rar)**
- **[Download Quoridor for Android](https://github.com/whyt1/Quoridor/releases/download/Android_Build/Quoridor_Android.apk)**

## Project Overview

### Objective

The primary goal of this project is to demonstrate the application of game development concepts, including AI, networking, and game logic, within the Unity framework. The game showcases:

- **AI Implementation:** The AI logic for Quoridor is powered by a best-first search algorithm combined with a heuristic function. This implementation highlights knowledge in algorithms and graph theory.
- **Multiplayer Functionality:** The game supports online multiplayer gameplay using the Wrapapp API. This demonstrates the ability to work with third-party libraries, handle server requests and callbacks, and parse custom game data types into strings and JSON.

### Game Description

**Quoridor** is a strategy board game where players must move their pawn to the opposite side of the board while placing barriers to block their opponent. The game ends when one player successfully reaches the other side. The challenge lies in strategically placing barriers while navigating around those placed by the opponent.

### Key Features

- **AI-Driven Opponent:** The AI opponent uses a best-first search algorithm with a heuristic function to determine the most optimal moves.
- **Online Multiplayer:** Play against other players online, with real-time game synchronization via the Wrapapp API.
- **Custom Game Logic:** The game includes custom logic for parsing and handling game data, ensuring a seamless multiplayer experience.

## How to Play

- **Single Player:** Play against the AI, which will challenge you with its strategic barrier placement and movement.
- **Multiplayer:** Invite a friend or join a random match online. Take turns placing barriers and moving your pawn until one player reaches the other side.

## Installation

### Prerequisites

To build and run this project, you will need:

- **Unity 2022.3 LTS** (or later)

1. **Clone the repository:**
   ```bash
   git clone https://github.com/whyt1/Assignment_YamTamim.git
   ```
2. **Open the project in Unity:**
   - Launch Unity Hub and select the project folder.
   - Ensure that all dependencies are properly loaded.

3. **Build and Run:**
   - Choose your target platform and build the game.
   - Test the game locally or deploy it to your preferred platform.

## Technical Details

### AI Logic

The AI in Quoridor is implemented using a **best-first search algorithm**. This algorithm evaluates possible moves based on a heuristic function, which estimates the "cost" of each move in terms of how close it brings the AI to its goal. The heuristic takes into account the position of barriers and the distance to the goal, allowing the AI to make intelligent decisions.

### Networking

The game uses the **Wrapapp API** for all multiplayer functionality. This includes:

- **Player Matching:** Connecting players in real-time.
- **Game State Management:** Synchronizing game state between players.
- **Data Parsing:** Converting custom game data types to JSON for transmission over the network.

### Code Snippets

If you’d like to see a more detailed description of the code, please reach out, and I’d be happy to share snippets or explain specific parts of the implementation.

## Future Enhancements

- **Improved AI:** Further enhancements to the AI's decision-making process.
- **Enhanced UI:** A more polished user interface, including animations and effects.

## Contributing

Contributions are welcome! If you find any bugs or have suggestions for improvements, feel free to open an issue or submit a pull request.

## License

This project is licensed under the MIT License.

## Acknowledgements

- **Wrapapp API:** For providing the multiplayer server backend.
- **Unity Community:** For the wealth of resources and support available to developers.
