# **Product Requirements Document: Teens Church Management API**

## **1\. Overview and Objectives**

This backend system provides a robust, scalable API to manage the biodata, service attendance, and unit assignments of Teens Church members. It serves as the data layer for the frontend dashboard, handling all CRUD (Create, Read, Update, Delete) operations and providing a specialized bulk-import mechanism to digitize physical registration forms rapidly via CSV.

## **2\. Architecture & Technology Stack**

* **Framework:** C\# 14 with **.NET 10** (utilizing Minimal APIs for lightweight, high-performance routing).  
* **Data Access:** Entity Framework Core 10 (EF Core).  
* **Database:** PostgreSQL or SQL Server (depending on hosting environment).  
* **CSV Parsing:** CsvHelper (NuGet package) for reliable, strongly-typed CSV deserialization.  
* **Documentation:** Swagger/OpenAPI for frontend integration testing.

## **3\. Data Model (Entity Schema)**

The core entity represents a single teenager's registration profile.

| Field | Data Type | Constraints / Rules | Description |
| :---- | :---- | :---- | :---- |
| Id | Guid | Primary Key, Auto-generated | Unique identifier for the member. |
| FullName | string | Required, Max Length 100 | The teenager's first and last name. |
| Age | int? | Nullable | Stored as an integer; null represents "Not Provided". |
| PhoneNumber | string | Nullable, Max Length 20 | Contact number. |
| AcademicLevel | string | Nullable, Max Length 50 | E.g., SS1, JSS3, Admission Seeker. |
| Departments | string | Nullable | Comma-separated string or JSON array of assigned units (e.g., "Choir, Media"). |
| ServiceTime | string | Required, Max Length 20 | Indicates attendance block (e.g., "6:30 service", "8:30 service"). |
| CreatedAt | DateTime | Auto-generated | Timestamp of record creation. |

## **4\. Core API Endpoints**

### **Member Management (CRUD)**

* **GET /api/members**: Retrieves a paginated list of members.  
  * *Query Parameters:* ?search={name}, ?department={unit}, ?service={time}, ?page=1\&pageSize=50.  
* **GET /api/members/{id}**: Retrieves a specific member's full profile.  
* **POST /api/members**: Creates a single new member record.  
* **PUT /api/members/{id}**: Updates an existing member's information.  
* **DELETE /api/members/{id}**: Removes a member from the database.

### **Bulk Operations**

* **POST /api/members/bulk-upload**: Accepts a multipart/form-data file upload containing a CSV.

## **5\. Feature Deep-Dive: CSV Bulk Import**

To transition from physical paper forms to the digital system, the backend must support uploading a CSV file containing dozens or hundreds of rows at once.  
**Processing Workflow:**

> 1. **File Validation:** The system verifies the uploaded file is a valid .csv and does not exceed the maximum file size limit (e.g., 5MB).  
> 2. **Header Mapping:** The system maps CSV columns (Name, Age, Phone, Level, Interested Department, Service) to the C\# entity properties using CsvHelper Class Maps.  
> 3. **Data Sanitization:**  
   * Any text reading "Not Provided" or empty cells in the Age column are parsed as null.  
   * String inputs are trimmed of leading/trailing whitespace.  
> 4. **Database Transaction:** The parsed records are grouped and inserted into the database using AddRangeAsync within a single database transaction. If a critical database error occurs, the entire batch rolls back to prevent partial, corrupted uploads.  
> 5. **Response Payload:** The API returns a 200 OK summary object detailing the operation's success, which the frontend can display to the user.

**Example C\# Response Payload:**

JSON  
{  
  "totalProcessed": 82,  
  "successfulInserts": 80,  
  "failedRows": 2,  
  "errors": \[  
    { "row": 15, "reason": "FullName is required but was blank." },  
    { "row": 42, "reason": "Age format invalid (expected number)." }  
  \]  
}

## **6\. Security & Validation Requirements**

* **Input Validation:** Use FluentValidation to ensure incoming POST/PUT requests meet schema requirements (e.g., FullName cannot be empty).  
* **CORS (Cross-Origin Resource Sharing):** Configure the backend to explicitly allow requests from the designated frontend domain to prevent unauthorized access.  
* **Rate Limiting:** Implement basic rate limiting on the /bulk-upload endpoint to prevent server overload from repeated large file submissions.