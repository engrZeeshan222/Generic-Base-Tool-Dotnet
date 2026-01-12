# Postman Collection Guide

## 📦 Import Instructions

1. **Open Postman**
2. Click **Import** button (top left)
3. Select **File** tab
4. Choose `Patient_API_Postman_Collection.json`
5. Click **Import**

The collection will be imported with all 50+ endpoints organized in folders.

---

## 🔧 Setup

### 1. Set Base URL (Optional)

The collection includes a variable `{{baseUrl}}` set to `https://localhost:7001`.

To change it:
1. Click on collection name → **Variables** tab
2. Edit `baseUrl` value
3. Click **Save**

### 2. Required Headers

All requests include these headers by default:
- `X-Tenant-Id: 1`
- `X-User-Id: 100`
- `X-Role-Id: 5`

You can modify these in individual requests or create a collection-level header preset.

---

## 📁 Collection Structure

The collection is organized into 10 folders:

### 1. **Basic CRUD Operations**
- Create Patient
- Get Patient by ID
- Update Patient
- Get All Patients
- Get Active Patients
- Search Patients

### 2. **Bulk Operations**
- Create Patients Bulk (AddMany)
- Save or Update Patient (Upsert)
- Get Patients by IDs (ListAsync)
- Soft Delete Multiple Patients
- Hard Delete by Condition
- Remove List of Patients

### 3. **Query Operations**
- Count Patients
- Count Patients by MRN
- Count Active Patients
- Check Patient Exists

### 4. **Specification Pattern**
- Get Active Patients by Specification
- Get Patients by Age Range
- Get Patient by MRN
- Get Patients with Appointments

### 5. **Delete Operations**
- Soft Delete Patient
- Hard Delete Patient by ID
- Hard Delete Patient Entity

### 6. **Change Tracking**
- Get Change History (DetectChange)
- Get Full JSON Comparison

### 7. **Audit Operations**
- Set Audit Properties

### 8. **Advanced Filtering (BaseFilters)**
- Advanced Filters - All Properties
- Advanced Filters - Cross Tenant
- Advanced Filters - Include Soft Deleted

### 9. **Patient Status Operations**
- Activate Patient
- Deactivate Patient

### 10. **Transaction Management**
- Create Patients in Transaction
- Demo Transaction Rollback

---

## 🧪 Testing Workflow

### Step 1: Create Test Data

1. Run **"Create Patient"** from folder 1
2. Note the `id` from response (e.g., `1`)
3. Create 2-3 more patients for bulk operations

### Step 2: Test CRUD Operations

1. **Get Patient by ID** - Use the ID from step 1
2. **Update Patient** - Modify the patient data
3. **Get All Patients** - Verify all patients are listed
4. **Search Patients** - Test search functionality

### Step 3: Test Bulk Operations

1. **Create Patients Bulk** - Create multiple patients at once
2. **Get Patients by IDs** - Get specific patients by their IDs
3. **Save or Update Patient** - Test upsert operation

### Step 4: Test Query Operations

1. **Count Patients** - Get total count
2. **Count Patients by MRN** - Count with predicate
3. **Check Patient Exists** - Verify existence check

### Step 5: Test Specifications

1. **Get Active Patients by Specification**
2. **Get Patients by Age Range** (e.g., 25-45)
3. **Get Patient by MRN**
4. **Get Patients with Appointments**

### Step 6: Test Delete Operations

1. **Soft Delete Patient** - Soft delete a patient
2. **Get All Patients** - Verify it's excluded
3. **Get All Patients (includeSoftDeleted=true)** - Verify it's included
4. **Hard Delete Patient by ID** - Permanently delete

### Step 7: Test Change Tracking

1. **Get Change History** - See what changed
2. **Get Full JSON Comparison** - See old vs new data

### Step 8: Test Advanced Features

1. **Advanced Filters** - Test all BaseFilters properties
2. **Transaction Management** - Test transaction rollback
3. **Set Audit Properties** - Manually set audit fields

---

## 📝 Sample Request Bodies

### Create Patient Request
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

### Gender Values
- `1` = Male
- `2` = Female
- `3` = Other

---

## 🔍 Expected Responses

### Success Response (Create/Update)
```json
{
  "id": 1,
  "mrn": "MRN-2024-001",
  "firstName": "John",
  "lastName": "Doe",
  "createdBy": 100,
  "createdOn": "2024-01-02T10:30:00Z",
  "updatedBy": 100,
  "updatedOn": "2024-01-02T10:30:00Z",
  "tenantId": 1
}
```

### Error Response
```json
{
  "error": "MRN 'MRN-2024-001' already exists in this facility."
}
```

---

## ⚠️ Important Notes

1. **SSL Certificate**: If you get SSL errors, disable SSL verification in Postman:
   - File → Settings → General → SSL certificate verification (OFF)

2. **Unique MRNs**: Each patient must have a unique MRN. If you get "MRN already exists" error, use a different MRN.

3. **Tenant Isolation**: 
   - Patients created with `X-Tenant-Id: 1` are only visible to Tenant 1
   - Change `X-Tenant-Id` header to test multi-tenancy

4. **User IDs**: 
   - `X-User-Id` is used for audit tracking (CreatedBy, UpdatedBy)
   - Change this header to see different users in audit fields

5. **Soft Delete**: 
   - Soft deleted patients are excluded from normal queries
   - Use `includeSoftDeleted=true` to see them

---

## 🎯 Testing Checklist

Use this checklist to ensure you've tested all features:

### Basic Operations
- [ ] Create patient
- [ ] Get patient by ID
- [ ] Update patient
- [ ] Get all patients
- [ ] Search patients

### Bulk Operations
- [ ] Create multiple patients (bulk)
- [ ] Get patients by IDs
- [ ] Save or update (upsert)
- [ ] Soft delete multiple
- [ ] Hard delete by condition
- [ ] Remove list

### Query Operations
- [ ] Count all patients
- [ ] Count with predicate
- [ ] Check existence

### Specifications
- [ ] Active patients specification
- [ ] Age range specification
- [ ] MRN specification
- [ ] Appointments specification

### Delete Operations
- [ ] Soft delete
- [ ] Hard delete by ID
- [ ] Hard delete entity

### Change Tracking
- [ ] Detect changes
- [ ] Full JSON comparison

### Audit Operations
- [ ] Set audit properties manually

### Advanced Filtering
- [ ] Filter by CreatedBy
- [ ] Filter by UpdatedBy
- [ ] Filter by date range
- [ ] Cross-tenant query
- [ ] Include soft deleted
- [ ] Sorting

### Transactions
- [ ] Create in transaction
- [ ] Transaction rollback

---

## 🚀 Quick Test Sequence

Run these in order for a quick test:

1. **Create Patient** (folder 1)
2. **Get Patient by ID** (use ID from step 1)
3. **Update Patient** (use ID from step 1)
4. **Count Patients** (folder 3)
5. **Get Change History** (folder 6, use ID from step 1)
6. **Soft Delete Patient** (folder 5, use ID from step 1)
7. **Get All Patients** (should not show deleted patient)
8. **Get All Patients with includeSoftDeleted=true** (should show deleted patient)

---

## 📚 Additional Resources

- **Swagger UI**: https://localhost:7001
- **API Documentation**: See `100_PERCENT_UTILIZATION_COMPLETE.md`
- **Quick Start Examples**: See `QUICK-START-EXAMPLES.md`

---

**Happy Testing!** 🎉

If you encounter any issues, check:
1. API is running on `https://localhost:7001`
2. Database is created and migrated
3. Headers are set correctly
4. MRN values are unique

