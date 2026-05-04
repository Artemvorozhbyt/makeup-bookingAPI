# ⚙️ Makeup Booking API

RESTful backend API for a full-stack booking platform designed for a private makeup studio.
Handles appointment scheduling, availability logic, client reviews, authentication, and email notifications.

---

✨ Features

📅 Appointment booking with real-time availability validation
⏱️ Time slot conflict detection (no double bookings)
💬 Client reviews system
📧 Automated email notifications (Brevo API)
🔐 JWT authentication with role-based authorization (Admin)
🗄️ PostgreSQL database with Entity Framework Core
🌐 CORS configuration for frontend integration

---

🏗️ Tech Stack

ASP.NET Core Web API (.NET 8)
Entity Framework Core
PostgreSQL (Render)
JWT Authentication
Brevo Email API

---

🧠 Architecture

Clean REST API design
Separation of concerns:

  * Controllers (API endpoints)
  * Services (email logic)
  * Data layer (EF Core DbContext)
* DTO-based request handling
* Environment-based configuration

---

📅 Booking Logic

Only predefined time slots allowed
Prevents overlapping appointments
Converts local time → UTC for storage
Validates:

  * Email format
  * Phone number
  * Future date

---

🔐 Authentication & Authorization

JWT-based authentication
Role-based access control:

  * `Admin` → full access (view/delete bookings)
  * Anonymous → create booking, view availability

---

📧 Email Integration

Integrated **Brevo API** (SMTP alternative)

Sends:

  * Booking confirmation to client
  * Notification to admin

Handles:

  * API authentication via environment variables
  * Error logging
  * Background sending (non-blocking requests)

---

📡 API Endpoints

Booking


POST /api/booking
GET /api/booking/available
GET /api/booking/month
DELETE /api/booking/{id}   (Admin only)
GET /api/booking           (Admin only)

---

Reviews


GET /api/reviews
POST /api/reviews
DELETE /api/reviews/{id}

---

Auth


POST /api/auth/login
POST /api/auth/register

---

🗄️ Database

PostgreSQL hosted on Render
Code-first approach (EF Core)
Automatic migrations on startup

Entities:

* Users
* Bookings
* Services
* Reviews

---

## ⚙️ Environment Variables


ConnectionStrings__DefaultConnection=your_database_url
Jwt__Key
Jwt__Issuer
Jwt__Audience
Brevo__ApiKey


---

🚀 Deployment

Hosted on **Render**
Automatic deploy from GitHub
Runs on port `8080`
Configured for production environment

---

---

💡 Highlights

Real-world business logic (booking conflicts, scheduling)
External API integration (email service)
Secure authentication system
Production deployment with real database
Full integration with frontend

---

## 🔗 Related Project

👉 Frontend (UI):
https://github.com/Artemvorozhbyt/luxury-makeup-frontend

---

👤 Author

**Artem Vorozhbyt**

GitHub: https://github.com/Artemvorozhbyt
Email: artvorozhbyt@gmail.com

---
