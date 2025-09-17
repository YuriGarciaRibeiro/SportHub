# 🌱 SportHub - Seeder Data

This document describes all data that is automatically created by SportHub seeders during database initialization.

## 📋 Table of Contents

- [👤 Users](#-users)
- [🏢 Establishments](#-establishments)
- [⚽ Sports](#-sports)
- [🏟️ Courts](#-courts)
- [📅 Reservations](#-reservations)
- [⭐ Reviews](#-reviews)

---

## 👤 Users

### 🔑 Administrator
The system automatically creates **1 administrator user** with credentials configured in `appsettings.json`:

- **Email**: Configured via `AdminUserSettings:Email`
- **Password**: Configured via `AdminUserSettings:Password`
- **Name**: Configured via `AdminUserSettings:FirstName` and `LastName`
- **Role**: `Admin`

### 🏢 Establishment Members
**3 users** with `EstablishmentMember` role:

| Name | Email | Password | ID |
|------|-------|----------|-----|
| John Smith | john.smith@sporthub.com | SportHub123! | 11111111-1111-1111-1111-111111111111 |
| Mary Johnson | mary.johnson@sporthub.com | SportHub123! | 22222222-2222-2222-2222-222222222222 |
| Charles Williams | charles.williams@sporthub.com | SportHub123! | 33333333-3333-3333-3333-333333333333 |

### 👥 Regular Users
**5 users** with `User` role:

| Name | Email | Password | ID |
|------|-------|----------|-----|
| Anna Brown | anna.brown@email.com | SportHub123! | 44444444-4444-4444-4444-444444444444 |
| Peter Davis | peter.davis@email.com | SportHub123! | 55555555-5555-5555-5555-555555555555 |
| Lucy Miller | lucy.miller@email.com | SportHub123! | 66666666-6666-6666-6666-666666666666 |
| Robert Wilson | robert.wilson@email.com | SportHub123! | 77777777-7777-7777-7777-777777777777 |
| Emily Moore | emily.moore@email.com | SportHub123! | 88888888-8888-8888-8888-888888888888 |

**Total users**: 9 (1 admin + 3 establishment members + 5 users)

---

## 🏢 Establishments

The system creates **25+ establishments** located in Aracaju/SE, each associated with a specific member and offering different sports.

### 🌟 Main Establishments

#### 1. SportHub Arena Central
- **Manager**: John Smith
- **Location**: Rua João Pessoa, 450 - Centro, Aracaju/SE
- **Sports**: Football, Basketball, Volleyball
- **Description**: Complete sports complex in downtown Aracaju

#### 2. Club Premium Atalaia
- **Manager**: Mary Johnson
- **Location**: Avenida Santos Dumont, 1500 - Atalaia, Aracaju/SE
- **Sports**: Tennis, Padel
- **Description**: Premium club focused on tennis and padel at Atalaia Beach

#### 3. Centro Esportivo Grageru
- **Manager**: Charles Williams
- **Location**: Rua Campos, 800 - Grageru, Aracaju/SE
- **Sports**: Futsal, Handball
- **Description**: Modern sports center in Grageru neighborhood

### 📍 Geographic Distribution
Establishments are distributed across the main neighborhoods of Aracaju:
- Centro (Downtown)
- Atalaia
- Grageru
- Treze de Julho
- Jardins
- São José
- Salgado Filho
- Coroa do Meio
- Ponto Novo
- Farolândia

**Total establishments**: 25+

---

## ⚽ Sports

The system creates **8 sports modalities**:

| Name | Description | ID |
|------|-------------|-----|
| **Football** | Sport played on grass field with two teams of 11 players | A1111111-1111-1111-1111-111111111111 |
| **Basketball** | Sport played on indoor court by two teams of 5 players | A2222222-2222-2222-2222-222222222222 |
| **Volleyball** | Sport played on court divided by net, with two teams of 6 players | A3333333-3333-3333-3333-333333333333 |
| **Tennis** | Racquet sport played individually or in pairs | A4444444-4444-4444-4444-444444444444 |
| **Futsal** | Variant of football played indoors, with two teams of 5 players | A5555555-5555-5555-5555-555555555555 |
| **Handball** | Team sport played with hands, two teams of 7 players | A6666666-6666-6666-6666-666666666666 |
| **Padel** | Racquet sport played in doubles on enclosed court | A7777777-7777-7777-7777-777777777777 |
| **Beach Tennis** | Racquet sport played on sand without letting the ball bounce | A8888888-8888-8888-8888-888888888888 |

---

## 🏟️ Courts

### 📊 Dynamic Generation
The system automatically generates **multiple courts** for each establishment based on offered sports:

- **Football**: 1-2 courts per establishment
- **Basketball**: 2-3 courts per establishment  
- **Volleyball**: 2-3 courts per establishment
- **Tennis**: 2-4 courts per establishment
- **Futsal**: 3-4 courts per establishment
- **Handball**: 1-2 courts per establishment
- **Padel**: 2-4 courts per establishment
- **Beach Tennis**: 2-3 courts per establishment

### 🏷️ Naming Convention
Courts follow realistic naming patterns:
- **Football**: "Main Field", "Secondary Field"
- **Basketball**: "Court A", "Court B", "Court C"
- **Tennis**: "Court 1", "Court 2", "Central Court"
- **Futsal**: "Court 1", "Court 2", "Court 3", "Court 4"

### 💰 Pricing
- Prices range from **$10 - $50 per slot**
- Each slot represents **30 minutes** of usage
- Prices are randomly generated for each court

**Estimated total courts**: 200+

---

## 📅 Reservations

### 🔄 Automatic Generation
The system automatically creates reservations for **all users** (including admin and establishment members):

### 📊 Distribution
- **Each user receives**: 1-4 random reservations
- **Time period**: 10 days in the past to 15 days in the future
- **Hours**: Between 8 AM and 10 PM
- **Duration**: 1-3 hours per reservation

### 💵 Pricing
- **Price per hour**: $10 - $50 (based on court type)
- **Calculation**: Price/hour × reservation duration
- **Slots**: Calculated as duration × 2 (30-min slots)

### 📈 Expected Volume
- **9 users** × 1-4 reservations = **18-36 reservations** approximately
- Distributed among **200+ courts** in **25+ establishments**

---

## ⭐ Reviews

### 🎯 Complete Coverage
The system generates reviews for **all establishments**:

### 👥 Participation
- **Reviewers**: Only users with `User` role (5 users)
- **Per establishment**: 3-6 random reviews
- **Estimated total**: 75-150 reviews

### ⭐ Rating System
**Realistic distribution based on establishment type**:

#### 🏆 Premium/Elite Establishments
- 10% receive 3 stars
- 25% receive 4 stars  
- 65% receive 5 stars

#### 🏢 Centro/Club Establishments
- 5% receive 2 stars
- 15% receive 3 stars
- 40% receive 4 stars
- 40% receive 5 stars

#### 🏟️ General Establishments
- 3% receive 1 star
- 8% receive 2 stars
- 20% receive 3 stars
- 50% receive 4 stars
- 19% receive 5 stars

### 💬 Comments
**70% of reviews include comments**:

#### 🌟 Positive Comments (4-5 stars)
- "Excellent establishment! Modern and well-maintained facilities."
- "Great experience! Very attentive staff and courts in perfect condition."
- "Amazing place to practice sports. Highly recommend!"

#### 😐 Neutral Comments (3 stars)  
- "Okay place, meets basic needs."
- "Reasonable facilities, nothing exceptional."
- "Pleasant environment, but could improve."

#### 😞 Negative Comments (1-2 stars)
- "Facilities need maintenance."
- "Service leaves something to be desired."
- "Price a bit steep for what it offers."

---

## 🚀 How to Run Seeders

### 🔧 Configuration
1. Configure admin credentials in `appsettings.json`
2. Run database migrations
3. Start the application

### ⚡ Automatic Execution
Seeders run automatically on application startup in the following order:

1. **UserSeeder** (Order: 1) - Creates admin and test users
2. **SportSeeder** (Order: 2) - Creates sports modalities  
3. **EstablishmentSeeder** (Order: 3) - Creates establishments
4. **CourtSeeder** (Order: 4) - Creates courts for each establishment
5. **ReservationSeeder** (Order: 5) - Creates reservations for all users
6. **EvaluationSeeder** (Order: 6) - Creates reviews for all establishments

### 🔄 Duplication Protection
- All seeders check if data already exists before creating
- Safe execution on multiple initializations
- Detailed logging of all operations

---

## 📊 Data Summary

| Category | Quantity | Characteristics |
|----------|----------|-----------------|
| **👤 Users** | 9 | 1 admin + 3 establishment members + 5 users |
| **🏢 Establishments** | 25+ | Distributed across Aracaju/SE |
| **⚽ Sports** | 8 | Popular modalities in Brazil |
| **🏟️ Courts** | 200+ | Dynamically generated per establishment |
| **📅 Reservations** | 18-36 | Distributed between past and future |
| **⭐ Reviews** | 75-150 | Realistic ratings with comments |

---

*This document is automatically updated according to seeder modifications.*