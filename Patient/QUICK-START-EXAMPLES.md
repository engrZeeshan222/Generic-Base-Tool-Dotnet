# Patient Microservice - Quick Start Examples

## Quick Test Commands

### 1. Start the API

```bash
cd Patient
dotnet run --project Patient.API/Patient.API.csproj
```

Wait for: **"Now listening on: https://localhost:7001"**

### 2. Open Swagger

Navigate to: **https://localhost:7001**

---

## Sample Data for Testing

Copy and paste these JSON payloads into Swagger UI or use with cURL/Postman.

### Create Patient #1 - John Doe

```json
{
  "mrn": "MRN-2024-001",
  "patientCode": "PAT-001",
  "firstName": "John",
  "lastName": "Doe",
  "dateOfBirth": "1990-01-15",
  "gender": 1,
  "phone": "555-0100",
  "email": "john.doe@hospital.com",
  "address": {
    "street": "123 Main Street",
    "city": "New York",
    "state": "NY",
    "zipCode": "10001",
    "country": "USA"
  },
  "emergencyContact": {
    "name": "Jane Doe",
    "relationship": "Spouse",
    "phone": "555-0101",
    "email": "jane.doe@hospital.com"
  },
  "bloodType": "O+",
  "allergies": "Penicillin, Pollen"
}
```

**Headers to use:**
```
X-Tenant-Id: 1
X-User-Id: 100
X-Role-Id: 5
```

---

### Create Patient #2 - Sarah Smith

```json
{
  "mrn": "MRN-2024-002",
  "patientCode": "PAT-002",
  "firstName": "Sarah",
  "lastName": "Smith",
  "dateOfBirth": "1985-06-20",
  "gender": 2,
  "phone": "555-0200",
  "email": "sarah.smith@hospital.com",
  "address": {
    "street": "456 Oak Avenue",
    "city": "Los Angeles",
    "state": "CA",
    "zipCode": "90001",
    "country": "USA"
  },
  "emergencyContact": {
    "name": "Michael Smith",
    "relationship": "Brother",
    "phone": "555-0201",
    "email": "michael.smith@hospital.com"
  },
  "bloodType": "A+",
  "allergies": "None"
}
```

---

### Create Patient #3 - Robert Johnson

```json
{
  "mrn": "MRN-2024-003",
  "patientCode": "PAT-003",
  "firstName": "Robert",
  "lastName": "Johnson",
  "dateOfBirth": "1975-11-30",
  "gender": 1,
  "phone": "555-0300",
  "email": "robert.johnson@hospital.com",
  "address": {
    "street": "789 Elm Street",
    "city": "Chicago",
    "state": "IL",
    "zipCode": "60601",
    "country": "USA"
  },
  "emergencyContact": {
    "name": "Lisa Johnson",
    "relationship": "Wife",
    "phone": "555-0301",
    "email": "lisa.johnson@hospital.com"
  },
  "bloodType": "B+",
  "allergies": "Sulfa drugs"
}
```

---

### Update Patient (Example)

**Endpoint**: `PUT /api/Patients/1`

```json
{
  "id": 1,
  "firstName": "John",
  "lastName": "Doe",
  "dateOfBirth": "1990-01-15",
  "gender": 1,
  "phone": "555-0100",
  "email": "john.doe.updated@hospital.com",
  "address": {
    "street": "123 Main Street Apt 4B",
    "city": "New York",
    "state": "NY",
    "zipCode": "10001",
    "country": "USA"
  },
  "emergencyContact": {
    "name": "Jane Doe",
    "relationship": "Spouse",
    "phone": "555-0105",
    "email": "jane.doe@hospital.com"
  },
  "bloodType": "O+",
  "allergies": "Penicillin, Pollen, Latex",
  "medicalNotes": "Patient has history of allergic reactions. Use caution with medications."
}
```

**Headers:**
```
X-Tenant-Id: 1
X-User-Id: 105    (different user to see UpdatedBy change)
X-Role-Id: 5
```

---

## cURL Examples

### Create Patient

```bash
curl -X POST "https://localhost:7001/api/Patients" \
  -H "Content-Type: application/json" \
  -H "X-Tenant-Id: 1" \
  -H "X-User-Id: 100" \
  -H "X-Role-Id: 5" \
  -k \
  -d '{
    "mrn": "MRN-2024-001",
    "patientCode": "PAT-001",
    "firstName": "John",
    "lastName": "Doe",
    "dateOfBirth": "1990-01-15",
    "gender": 1,
    "phone": "555-0100",
    "email": "john.doe@hospital.com",
    "address": {
      "street": "123 Main Street",
      "city": "New York",
      "state": "NY",
      "zipCode": "10001",
      "country": "USA"
    },
    "emergencyContact": {
      "name": "Jane Doe",
      "relationship": "Spouse",
      "phone": "555-0101",
      "email": "jane.doe@hospital.com"
    },
    "bloodType": "O+",
    "allergies": "Penicillin"
  }'
```

### Get All Patients

```bash
curl -X GET "https://localhost:7001/api/Patients" \
  -H "X-Tenant-Id: 1" \
  -H "X-User-Id: 100" \
  -H "X-Role-Id: 5" \
  -k
```

### Get Patient by ID

```bash
curl -X GET "https://localhost:7001/api/Patients/1" \
  -H "X-Tenant-Id: 1" \
  -H "X-User-Id: 100" \
  -H "X-Role-Id: 5" \
  -k
```

### Search Patients

```bash
curl -X GET "https://localhost:7001/api/Patients/search?searchTerm=john" \
  -H "X-Tenant-Id: 1" \
  -H "X-User-Id: 100" \
  -H "X-Role-Id: 5" \
  -k
```

### Soft Delete Patient

```bash
curl -X DELETE "https://localhost:7001/api/Patients/1" \
  -H "X-Tenant-Id: 1" \
  -H "X-User-Id: 100" \
  -H "X-Role-Id: 5" \
  -k
```

### Activate/Deactivate Patient

```bash
# Deactivate
curl -X POST "https://localhost:7001/api/Patients/1/deactivate" \
  -H "X-Tenant-Id: 1" \
  -H "X-User-Id: 100" \
  -H "X-Role-Id: 5" \
  -k

# Activate
curl -X POST "https://localhost:7001/api/Patients/1/activate" \
  -H "X-Tenant-Id: 1" \
  -H "X-User-Id: 100" \
  -H "X-Role-Id: 5" \
  -k
```

---

## Testing Generic Toolkit Features

### Test 1: Multi-Tenancy Isolation

**Step 1**: Create patient for Tenant 1
```
POST /api/Patients
X-Tenant-Id: 1
X-User-Id: 100
(use sample data above)
```

**Step 2**: Try to get patient from Tenant 2
```
GET /api/Patients/1
X-Tenant-Id: 2
X-User-Id: 200
```

**Expected Result**: 404 Not Found (tenant isolation working)

---

### Test 2: Audit Tracking

**Step 1**: Create patient
```
POST /api/Patients
X-User-Id: 100
(Created by User 100)
```

**Step 2**: Update patient
```
PUT /api/Patients/1
X-User-Id: 105
(Updated by User 105)
```

**Step 3**: View patient
```
GET /api/Patients/1
```

**Check Response**:
```json
{
  "createdBy": 100,
  "createdOn": "2024-01-15T10:30:00Z",
  "updatedBy": 105,
  "updatedOn": "2024-01-15T14:20:00Z"
}
```

---

### Test 3: Soft Delete

**Step 1**: Create patient
```
POST /api/Patients
```

**Step 2**: Soft delete
```
DELETE /api/Patients/1
X-User-Id: 100
```

**Step 3**: Try to get patient
```
GET /api/Patients/1
```

**Expected**: 404 Not Found

**Step 4**: Get including soft deleted
```
GET /api/Patients?includeSoftDeleted=true
```

**Expected**: Patient visible with `isDeleted: true, deletedBy: 100`

---

### Test 4: Transaction Rollback

**Create batch** (second patient has duplicate MRN):

```
POST /api/TransactionDemo/create-batch

[
  {
    "mrn": "MRN-BATCH-001",
    "patientCode": "BATCH-001",
    "firstName": "Test1",
    "lastName": "Patient1",
    "dateOfBirth": "1990-01-01",
    "gender": 1,
    "phone": "555-1111",
    "email": "test1@test.com",
    "address": { "street": "St1", "city": "City", "state": "ST", "zipCode": "00001", "country": "USA" },
    "emergencyContact": { "name": "Contact1", "relationship": "Friend", "phone": "555-1112", "email": "contact1@test.com" }
  },
  {
    "mrn": "MRN-BATCH-001",
    "patientCode": "BATCH-002",
    "firstName": "Test2",
    "lastName": "Patient2",
    "dateOfBirth": "1991-01-01",
    "gender": 2,
    "phone": "555-2222",
    "email": "test2@test.com",
    "address": { "street": "St2", "city": "City", "state": "ST", "zipCode": "00002", "country": "USA" },
    "emergencyContact": { "name": "Contact2", "relationship": "Friend", "phone": "555-2223", "email": "contact2@test.com" }
  }
]
```

**Expected**: Transaction fails and rolls back (duplicate MRN). Neither patient is created.

---

### Test 5: Specification Pattern

**Get active patients**:
```
GET /api/Patients/by-specification/active
```

**Get patients by age range** (25-45 years old):
```
GET /api/Patients/by-age-range?minAge=25&maxAge=45
```

---

### Test 6: Change Tracking

**Step 1**: Get patient
```
GET /api/Patients/1
```

**Step 2**: Get change history (modifies Phone temporarily then restores)
```
GET /api/Patients/1/change-history
```

**Response**: JSON showing what changed (Phone field)

---

### Test 7: Pagination

```
GET /api/Patients?skip=0&take=2     # First 2 patients
GET /api/Patients?skip=2&take=2     # Next 2 patients
GET /api/Patients?skip=4&take=2     # Next 2 patients
```

---

## Verification Checklist

After running tests, verify:

- [ ] Patients created with correct TenantId
- [ ] CreatedBy/UpdatedBy populated correctly
- [ ] CreatedOn/UpdatedOn timestamps present
- [ ] Multi-tenant isolation working (Tenant 1 can't see Tenant 2 data)
- [ ] Soft delete marks IsDeleted=true
- [ ] Soft deleted patients excluded from normal queries
- [ ] Transaction rollback works
- [ ] Specification pattern returns correct results
- [ ] Search works case-insensitively
- [ ] Pagination works correctly
- [ ] Change tracking detects modifications

---

## Common Issues

**Issue**: "Cannot open database"
**Fix**: Run `dotnet ef database update` first

**Issue**: "MRN already exists"
**Fix**: Use unique MRN values (e.g., MRN-2024-001, MRN-2024-002)

**Issue**: "Tenant filter not working"
**Fix**: Ensure X-Tenant-Id header is sent with every request

**Issue**: "Audit fields are null"
**Fix**: Ensure X-User-Id header is sent (defaults to 0 if missing)

---

**Happy Testing!** 🎉

All endpoints are documented in Swagger UI at https://localhost:7001
