# Contract Monthly Claim System (CMCS) — Part 2 

GitHub Repository link: https://github.com/Nkosi-Mmasebotsana/LecturerClaimsPrototypePart2.git 
Youtube video link: https://youtu.be/BChCqr4xWPk 


This repository contains **Part 2** of the CMCS project, which focuses on implementing the functionality of the application/system.

---

## 📌 Overview  
The Contract Monthly Claim System is a web-based ASP.NET Core MVC application that allows university lecturers to submit their monthly claims for payment, and for programme coordinators or academic managers to approve or reject these claims.

This project (Part 2) focuses on core claim management functionality, including submission, approval, rejection, and supporting document uploads, along with unit testing and error handling to ensure consistent and reliable behavior. 

---

## 🖼️ Current Features  

 **Lecturer Functionality**  
-  View lecturer dashboard with submitted claims.
- Create new claims with multiple claim lines.
- Upload supporting documents (PDF, DOCX, XLSX, JPG, PNG).
- Automatic calculation of total hours and total claim amount.

 **Approver Functionality**  
- View pending claims in the approver dashboard.
- Approve or reject claims with comments.
- Track approval history and rejected claims.
  
**Administrator Functionality**  
- Manage lecturers (add new lecturers).
- View all lecturers in a list.

**System Features**  
- Supports multiple claim lines per claim.
- File upload validation (type and size).
- Robust error handling for invalid data or missing claims.
- Temporary data messages for success/error feedback.
  
---

## Technologies Used
- ASP.NET Core MVC (C#)
- Razor Views for front-end UI
- Moq & xUnit for unit testing
- In-memory data storage for lecturers, users, and claims (static lists)
- File uploads stored in wwwroot/uploads
  
---

## Setup and Installation

(1) Clone the repository:  
(2) Open the solution in Visual Studio 2022.
(3) Restore NuGet packages if necessary.
(4) Ensure the wwwroot/uploads folder exists for document uploads.
(5) Build and run the project.  

---

## Usage
1. Lecturer
- Navigate to /Claim/LecturerDashboard?lecturerId=101 to view claims.
- Click “Create Claim” to submit a new claim.
- Add multiple claim lines and upload supporting documents.

2. Approver (Programme Coordinator / Academic Manager)
- Navigate to /Claim/ApproverDashboard to view pending claims.
- Approve or reject claims with optional comments.

3. Administrator
- Navigate to /Claim/LecturersList to view lecturers.
- Click “Add Lecturer” to create new lecturer records.
  
---

## Unit Testing 

- Unit tests are implemented using xUnit and Moq.

- All key functionalities are tested, including:
(1) Adding lecturers (valid/invalid).
(2) Submitting claims with multiple lines and documents.
(3) Approving and rejecting claims.
(4) Dashboard data retrieval.
(5) Error handling scenarios (non-existing lecturer/claim, invalid files, missing data).

Run tests from Visual Studio Test Explorer or using command line:
dotnet test 

---

## Error Handling
The system includes robust error handling to ensure consistency and reliability:
- Claims cannot be submitted for non-existing lecturers.
- Rejection requires a comment.
- File uploads are validated for size and type.
- All exceptions are caught and user-friendly messages are displayed using TempData.

---

## Notes
- Data is stored in in-memory static lists, suitable for demonstration purposes.
- Uploaded files are stored locally under wwwroot/uploads.
- Lecturer and user data are pre-populated for testing.
- Used chatgpt to write the unit tests

---

## 👨‍💻 Author  

- **Project Developer:** Mmasebotsana Nkosi   
- **Deliverable:** Part 2 — Contract Monthly Claim System
