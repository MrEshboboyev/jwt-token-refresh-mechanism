# 🔐 JWT Token Refresh Mechanism – Secure API Authentication 🚀  

![.NET 9](https://img.shields.io/badge/.NET%209-blue?style=for-the-badge)
![JWT](https://img.shields.io/badge/JWT%20Authentication-%F0%9F%94%92-red?style=for-the-badge)
![Clean Architecture](https://img.shields.io/badge/Clean%20Architecture-%F0%9F%9A%80-green?style=for-the-badge)
![Domain-Driven Design](https://img.shields.io/badge/DDD-%F0%9F%93%9A-yellow?style=for-the-badge)
![Security](https://img.shields.io/badge/Secure%20API-%E2%9C%94%EF%B8%8F-blue?style=for-the-badge)

A **secure and scalable JWT Token Refresh Mechanism** built with **.NET 9**, **Domain-Driven Design (DDD)**, and **Clean Architecture**. Implements **refresh tokens, token revocation, and multi-layered authentication** to enhance security and user management.  

---

## 🔥 Features  

✅ **User Registration & Login** – Secure user authentication with password hashing.  
✅ **JWT Access Tokens** – Short-lived access tokens for secure API access.  
✅ **Refresh Tokens** – Long-lived refresh tokens for session continuation.  
✅ **Token Revocation** – Prevent unauthorized access by revoking refresh tokens.  
✅ **DDD & Clean Architecture** – Ensures maintainability and scalability.  
✅ **Unit Testing** – Comprehensive tests for security and authentication workflows.  

---

## 🛠 Technologies Used  

🟣 **.NET 9** – Backend framework for secure API development.  
🔴 **JWT Authentication** – Secure user authentication and authorization.  
🟢 **Entity Framework Core** – Database interactions and migrations.  
🟡 **FluentValidation** – Request validation to enforce strong security rules.  
🔵 **MediatR** – Implements **CQRS** for command-query separation.  
🟠 **xUnit & Moq** – Unit testing and mocking dependencies for reliability.  

---

## 🏛️ Architecture  

This project follows **Clean Architecture** principles and **DDD**:  

**1️⃣ Domain Layer**  
📌 Business logic, entities, and domain events (**User, RefreshToken**).  

**2️⃣ Application Layer**  
📌 Use cases, commands, queries (**LoginCommand, RefreshTokenCommand**).  

**3️⃣ Infrastructure Layer**  
📌 Database, repositories, JWT provider, external services (**EF Core, TokenService**).  

**4️⃣ Presentation Layer**  
📌 API controllers, request/response models (**UsersController, LoginRequest**).  

---

## 🔄 How It Works  

### **1️⃣ User Registration**  
🔹 Users register with email and password.  
🔹 Passwords are **hashed** before being stored securely.  

### **2️⃣ Login & Token Issuance**  
🔹 Users log in with credentials.  
🔹 A **JWT access token** (short-lived) and a **refresh token** (long-lived) are issued.  
🔹 The refresh token is stored securely in the database.  

### **3️⃣ Token Refresh Flow**  
🔹 When the access token expires, the client sends a **refresh token** to `/refresh-token`.  
🔹 A new **JWT access token** is issued.  
🔹 The old refresh token is revoked, ensuring security.  

### **4️⃣ Token Revocation**  
🔹 Users can **revoke refresh tokens**, logging out from all devices.  

---

## 🚀 Getting Started  

### **Prerequisites**  
📌 [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)  
📌 [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)  
📌 [Postman](https://www.postman.com/) for API testing  

### **Step 1: Clone the Repository**  
```bash
git clone https://github.com/MrEshboboyev/jwt-token-refresh-mechanism.git
cd jwt-token-refresh-mechanism
```

### **Step 2: Install Dependencies**  
```bash
dotnet restore
```

### **Step 3: Run the Application**  
```bash
dotnet run --project src/Presentation
```

---

## 🔗 API Endpoints  

| Method | Endpoint                | Description |
|--------|-------------------------|-------------|
| **POST** | `/api/users/register`     | Registers a new user |
| **POST** | `/api/users/login`        | Logs in and returns access & refresh tokens |
| **POST** | `/api/users/refresh-token` | Refreshes JWT access token |
| **POST** | `/api/users/revoke-token`  | Revokes a refresh token |

---

## 🧪 Testing  

### **Unit Tests**  
Run all unit tests:  
```bash
dotnet test
```

### **Manual API Testing**  
1️⃣ **Register a new user**  
2️⃣ **Log in and get access & refresh tokens**  
3️⃣ **Use refresh token to obtain a new access token**  
4️⃣ **Revoke refresh token to log out**  

✅ **Test using Postman, Swagger, or any REST client**.  

---

## 🎯 Why Use This Project?  

✅ **High Security** – Implements **refresh token rotation & revocation**.  
✅ **Scalable & Maintainable** – Follows **DDD & Clean Architecture**.  
✅ **Ready for Production** – Follows industry best practices.  
✅ **Cloud-Ready** – Can be deployed on **AWS, Azure, Kubernetes**.  

---

## 📄 License  

This project is licensed under the **MIT License**. See the [LICENSE](LICENSE) file for details.  

---

## 📞 Contact  

For feedback, questions, or contributions:  
📧 **Email**: mreshboboyev@gmail.com  
💻 **GitHub**: [MrEshboboyev](https://github.com/MrEshboboyev)  

---

🚀 **Secure your API with a scalable JWT Token Refresh Mechanism!** Clone the repo and start coding today! 
