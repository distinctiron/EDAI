# EDAI – Essay Document AI

**EDAI** is a web-based application that leverages OpenAI's language models to assist students learning English as a foreign language by analyzing their Word document essays. The system provides feedback on grammar, eloquence, structure, and argumentation directly in the document and tracks progress over time.

---

## 🚀 Core Functionality

- Upload `.docx` essays and receive AI-generated feedback directly embedded in the document.
- Comments are inserted contextually using OpenXML and character index tracking.
- AI feedback covers grammar, phrasing, coherence, and overall argumentation.
- Dashboard for managing uploaded essays and reviewing progress over time.
- Progress reports and summary generation using SignalR and background job handling (via Hangfire).
- All feedback generation is processed through a private Azure-hosted OpenAI instance for GDPR compliance.

---

## 🧱 Architecture Overview

The solution is split into three projects:

### 1. **EDAI.Client** (Blazor WebAssembly Frontend)
- Built with [Blazor WebAssembly](https://learn.microsoft.com/en-us/aspnet/core/blazor/?view=aspnetcore-8.0&tabs=visual-studio).
- Uses [MudBlazor](https://mudblazor.com/) for UI components.
- Communicates with the backend API for document upload, processing, and data retrieval.
- Uses SignalR for real-time updates (e.g., when essay summaries are ready).
- Handles client-side caching and file persistence via IndexedDB.

### 2. **EDAI.Server** (ASP.NET Core Web API Backend)
- ASP.NET Core backend exposing REST endpoints.
- Handles file uploads, interacts with OpenXML to read/modify Word documents.
- Sends documents to Azure OpenAI for LLM analysis.
- Uses [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/) with a SQLite database for storing students, documents, and feedback history.
- Job scheduling and background processing powered by [Hangfire](https://www.hangfire.io/).
- Uses SignalR to notify clients of processing results.

### 3. **EDAI.Shared**
- Contains common models (DTOs, enums, validation attributes) shared between client and server.
- Ensures strong typing and consistency across the solution.

---

## 🎨 Frontend Styling with MudBlazor

The UI is built using [MudBlazor](https://mudblazor.com/), a modern Blazor component library with:

- Material Design components (buttons, dialogs, tables, etc.)
- Form handling with validation support via `MudForm`
- Responsive layout system using `MudGrid` and `MudItem`
- Integrated charts (MudCharts) for data visualization

---

## ⚙️ Getting Started

1. Clone the repository
2. Ensure you have [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) installed
3. Update `appsettings.json` with appropriate Azure OpenAI and database configuration
4. Run the solution from Visual Studio or use `dotnet run` from the command line

---

## 📂 Folder Structure

EDAI/

├── EDAI.Client/ # Blazor frontend

├── EDAI.Server/ # Backend API + EF + Hangfire

├── EDAI.Shared/ # Shared DTOs and models
