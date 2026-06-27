# Grace's Music School - Administrative Portal

A highly robust, console-based administrative portal designed to manage students, teachers, facilities, and lesson scheduling. Built with C#, this application features a **Proactive Constraint-Solving Engine** that mathematically eliminates the possibility of double-booking, ensuring a zero-error user experience.

---

## 🚀 Key Features

* **Smart Lesson Scheduling:** A constraint-solving algorithm dynamically generates available 30-minute time slots based on teacher availability, room capacity, and student enrollments.
* **Zero-Error UI:** Powered by `Spectre.Console`, the application replaces manual ID typing with interactive, color-coded dropdown menus.
* **Dynamic Enrollments:** Decouples students from single instruments, allowing students to hold multiple active enrollments at varying skill levels.
* **Robust File-Based Database:** Utilizes a custom flat-file repository system (`.csv`) for persistent data storage, eliminating the need for external database dependencies.
* **Safe Cancellations:** Universal abort commands allow administrators to safely back out of any workflow without corrupting data state.

---

## 🏗️ Architecture

The application is structured using **Clean Architecture** principles to strictly separate business logic from data access and presentation.

* **`GraceMusic.Core`**: The domain layer. Contains entity models (`Student`, `Teacher`, `Lesson`) and the core business logic (`SchedulingService`). No external dependencies.
* **`GraceMusic.Infrastructure`**: The persistence layer. Implements `IRepository<T>` using robust CSV parsing and file stream management.
* **`GraceMusic.UI`**: The composition root and presentation layer. Handles all interactive console routing and user inputs.
* **`GraceMusic.Tests`**: The verification layer. Uses **xUnit** and **Moq** to test business logic and constraint-solving constraints in complete isolation.

---

## ⚙️ Getting Started (For Developers)

### Prerequisites
* [.NET SDK 6.0](https://dotnet.microsoft.com/download) or higher.
* A terminal or command prompt.

### Installation & Execution
1. Clone the repository to your local machine.
2. Navigate to the root workspace directory.
3. Build the solution to restore dependencies:
   ```bash
   dotnet build
   ```
4. Running the application
   ```
   dotnet run --project GraceMusic.UI
   ```

---
## 📚 Documentation
[User Guide](https://github.com/thejpala/cs690-finalproject-gracemusicschool/wiki/User-Documentation-V1.0)

[Developer Document](https://github.com/thejpala/cs690-finalproject-gracemusicschool/wiki/Development-Document-V1.0)

[Deployment Document](https://github.com/thejpala/cs690-finalproject-gracemusicschool/wiki/Deployment-Document)

[Design Document](https://github.com/thejpala/cs690-finalproject-gracemusicschool/wiki/Design)

[Requirements Specificaitons](https://github.com/thejpala/cs690-finalproject-gracemusicschool/wiki)


   
