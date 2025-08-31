# Samvad: Contextual VR Language Learning Platform

**Samvad** (Sanskrit/Hindi for *"conversation"* or *"dialogue"*) is an immersive virtual reality platform designed to teach new languages through contextual interaction and conversation with an AI-powered tutor.  

This project is built on the **OpenXR** standard, ensuring a robust and scalable learning experience on a wide range of compatible VR headsets.

---

## 🌍 Scenario

The initial scene places the user in a **German vegetable market** where they can:
- Learn vocabulary  
- Practice conversational skills  
- Interact with objects and an intelligent NPC vendor  

---

## ✨ Features

- **Immersive VR Environment**  
  A fully interactive vegetable market scene built in Unity for OpenXR.

- **AI-Powered Tutor**  
  A friendly NPC vendor driven by OpenAI’s GPT-4o, providing context-aware lessons, feedback, and conversation.

- **Real-time Speech-to-Text**  
  Utilizes OpenAI’s Whisper API for fast and accurate transcription of the user’s speech.

- **High-Quality Text-to-Speech**  
  The NPC’s responses are spoken aloud using OpenAI’s TTS API for a natural and engaging interaction.

- **Contextual Interaction**  
  The AI’s responses are intelligently tailored to the specific objects the user is holding or interacting with.

- **Modular Language System**  
  Easily configurable to teach different languages by changing a few parameters in Unity and the backend.

---

## 🏗 Architecture

The project is split into two main components:  
1. **Unity Client (Frontend)**  
   - Manages the VR scene, user interactions (grabbing, speaking), and UI.  
   - Records audio and communicates with the backend via a simple REST API.  

2. **Node.js Server (Backend)**  
   - Acts as a secure wrapper for the OpenAI APIs.  
   - Exposes a single endpoint for the Unity client.  
   - Manages the AI’s persona and instructional logic.  
   - *(Maintained in a separate repository but essential for the Unity client to function.)*

---

## 🔄 API & Data Flow

The communication between the Unity client and the Node.js backend is a **stateless request-response cycle**.

### Unity → Backend
- Captures audio  
- Converts it to Base64  
- Sends JSON payload with language settings and context tag  

### Backend Processing
1. **Whisper API**: Audio → Transcription  
2. **Chat Completions (GPT-4o)**: Transcript + Context → Intelligent Response  
3. **TTS API**: AI’s text reply → Natural speech audio  

### Backend → Unity
- Bundles **text reply** + **audio reply** in a JSON response  

---

## 📦 Example Payloads

### Request (Unity → Backend)
```json
{
  "audioData": "UklGRiS....",
  "inputLanguage": "en",
  "targetLanguage": "de",
  "contextTag": "Potato"
}
````

### Response (Backend → Unity)

```json
{
  "reply": "Eine Kartoffel. (A potato.) Now, try asking me for one by saying: 'Eine Kartoffel, bitte.'",
  "audioReply": "UklGRiS...."
}
```

---

## ⚙️ Prerequisites

* [Unity Hub](https://unity.com/download) with **Unity 6 LTS** (or newer)
* OpenXR Plugin
* [Node.js](https://nodejs.org/) v18 or newer
* OpenAI API Key (with billing enabled)
* OpenXR-compatible VR headset (Meta Quest, Pico, HTC Vive, etc.) + link cable for development
* [ngrok](https://ngrok.com/) for exposing your local backend

---

## 🖥 Backend Setup

1. Navigate to the backend folder:

   ```bash
   cd backend
   ```

2. Create an environment file:

   ```bash
   echo 'OPENAI_API_KEY="sk-..."' > .env
   ```

3. Install dependencies:

   ```bash
   npm install
   ```

4. Start the server:

   ```bash
   node server.js
   ```

5. Expose locally with ngrok:

   ```bash
   ngrok http 3000
   ```

   Copy the public URL.

---

## 🎮 Unity Client Setup

1. **Configure Backend Connector**

   * In your main scene, locate the **AIManager** GameObject.
   * In the **Simple Backend Connector** component, paste your `ngrok` URL with `/api/converse`.
   * Set `Input Language` and `Target Language` codes (e.g., `en`, `de`).
   * Assign your `AudioSource` and `TextMeshPro` UI components.

2. **Implement Contextual Interaction**

   * Tag interactive objects (e.g., `"Potato"`, `"Tomato"`) in Unity.
   * Configure your XR interaction system events (`OnSelectEntered/Exited`) to call:

     * `SetContextFromTag(GameObject)`
     * `ClearContext()`

3. **Connect the Microphone**

   * Ensure the `MicController` script is in the scene.
   * Assign your **Record button** to call `MicController.ToggleRecording()`.
   * Assign `SimpleBackendConnector` to the MicController’s backend field.

---

## 🚀 How to Use

1. Start the backend server and `ngrok`.
2. Enter Play Mode in Unity (or build & run on Quest).
3. Press the **Record button**, speak, then press again to stop.
4. Pick up a tagged object (e.g., a potato) before speaking → AI gives **context-aware replies**.

---


