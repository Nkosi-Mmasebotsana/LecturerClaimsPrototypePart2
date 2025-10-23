# Contract Monthly Claim System (CMCS) — Part 1  

GitHub Repository link: https://github.com/Nkosi-Mmasebotsana/CMCS-Prototype 

This repository contains **Part 1** of the CMCS project, which focuses on setting up the **user interface (UI)** and a basic **MVC structure**.  

---

## 📌 Overview  
The goal of Part 1 is to present the GUI design and initial page structure for the Contract Monthly Claim System (CMCS).  
At this stage:  

- **No actual business logic or database connectivity has been implemented.**  
- **Dummy data** is displayed to simulate what the system will eventually show.  
- **Buttons and links are for simulation purposes only** — they do not add claims, update data, or perform real actions.  
- **Popup messages** (or redirects) are used to simulate user interactions.  

---

## 🖼️ Current Features  

- **Approver Dashboard (Prototype)**  
  - Displays a list of dummy claims for testing.  
  - Buttons (e.g., "View") are present but do not load real data or perform database operations.  
  - This dashboard is still under development — some routes or pages may not load yet.  

- **Lecturers Tab (Prototype)**  
  - Currently displays an error or empty page instead of the intended layout.  
  - The UI structure is in progress.  

- **Static Role Display**  
  - Uses a simulated `ViewBag.Role` to show which dashboard is being viewed.  

---

## 🧪 Development Status  

| Module / Feature         | Status             | Notes                                                                 |
|-------------------------|------------------|----------------------------------------------------------------------|
| Approver Dashboard View | 🚧 In Progress   | Shows dummy claims, buttons simulate actions but do not update data. |
| Lecturer Tab            | 🚧 In Progress   | Currently not displaying proper layout (still in development).       |
| Claim Actions           | 🟢 Simulated     | No real claim submission or approval occurs.                         |
| Database Integration    | ❌ Not Started   | Will be added in later phases of the project.                        |

---

## ⚠️ Known Issues  

- **Approver Dashboard cannot be fully accessed yet** (still being built).  
- **Lecturers tab shows an error** instead of a layout.  
- Buttons only display placeholder messages — **no data is saved or modified**.  

---

## 🔧 Setup & Run  

1. Clone this repository.  
2. Open the solution in Visual Studio.  
3. Run the project — it will launch the dummy UI in your browser.  
4. Navigate to the available dashboard pages to preview the layout and sample data.  

---

## 🗒️ Notes  

This is **not a fully functional application yet** — it is strictly a **prototype for demonstration purposes**.  
The goal of this part was to build the UI layout and wire up basic navigation, not to implement backend logic.  

Future parts will add:  
- Real database connectivity (SQL).  
- Claim submission, approval, and rejection workflows.  
- Proper lecturer management UI.  

---

## 👨‍💻 Author  

- **Project Developer:** Mmasebotsana Nkosi   
- **Deliverable:** Part 1 — GUI / MVC Structure (Prototype)

