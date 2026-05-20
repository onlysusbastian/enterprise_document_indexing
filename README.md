-- =========================================================
-- CLEAN RESET
-- =========================================================

DROP TABLE IF EXISTS public.indexed_documents CASCADE;
DROP TABLE IF EXISTS public.users CASCADE;

DROP SEQUENCE IF EXISTS public.indexed_documents_id_seq CASCADE;
DROP SEQUENCE IF EXISTS public.users_id_seq CASCADE;


-- =========================================================
-- TABLE: users
-- =========================================================

CREATE TABLE public.users
(
    id SERIAL PRIMARY KEY,

    username VARCHAR(50) UNIQUE NOT NULL,

    password_hash TEXT NOT NULL,

    role VARCHAR(20) DEFAULT 'User',

    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);


-- =========================================================
-- TABLE: indexed_documents
-- =========================================================

CREATE TABLE public.indexed_documents
(
    id SERIAL PRIMARY KEY,

    index_id TEXT UNIQUE NOT NULL,

    file_name TEXT NOT NULL,

    stored_file_name TEXT,

    file_path TEXT,

    file_size BIGINT,

    file_extension VARCHAR(20),

    region VARCHAR(50),

    doc_type VARCHAR(100),

    department VARCHAR(100),

    upload_year INT,

    upload_date DATE,

    upload_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    employee_assigned VARCHAR(150),

    project_name VARCHAR(200),

    uploader_identity VARCHAR(150),

    metadata_tags TEXT[],

    description TEXT
);


-- =========================================================
-- PERFORMANCE INDEXES
-- =========================================================

CREATE INDEX idx_documents_region
ON public.indexed_documents(region);

CREATE INDEX idx_documents_doc_type
ON public.indexed_documents(doc_type);

CREATE INDEX idx_documents_department
ON public.indexed_documents(department);

CREATE INDEX idx_documents_year
ON public.indexed_documents(upload_year);

CREATE INDEX idx_documents_project
ON public.indexed_documents(project_name);

CREATE INDEX idx_documents_employee
ON public.indexed_documents(employee_assigned);

CREATE INDEX idx_documents_tags
ON public.indexed_documents
USING GIN(metadata_tags);


-- =========================================================
-- USERS DATA
-- =========================================================

INSERT INTO public.users
(username, password_hash, role)
VALUES
('admin', 'admin123', 'Administrator'),

('employee1', 'emp123', 'User'),

('manager1', 'mgr123', 'Manager');


-- =========================================================
-- DOCUMENT DATA
-- =========================================================

INSERT INTO public.indexed_documents
(
    index_id,
    file_name,
    stored_file_name,
    file_path,
    file_size,
    file_extension,

    region,
    doc_type,
    department,

    upload_year,
    upload_date,

    employee_assigned,
    project_name,
    uploader_identity,

    metadata_tags,

    description
)
VALUES

(
    'TRX-10001',
    'Spool_Piece_Repair_Ledger_v1.xlsx',
    'stored_10001.xlsx',
    'D:/vault/TRX-10001.xlsx',
    512000,
    '.xlsx',

    'Northern',
    'XLSX',
    'Production',

    2025,
    '2025-05-01',

    'Amit Sharma',
    'Pipeline Integrity Audit',
    'admin',

    ARRAY['pipeline','repair','ledger','integrity'],

    'Repair tracking ledger for damaged spool pieces.'
),

(
    'TRX-10002',
    'Well_Log_Analysis_Report.pdf',
    'stored_10002.pdf',
    'D:/vault/TRX-10002.pdf',
    1048576,
    '.pdf',

    'Eastern',
    'PDF',
    'Exploration',

    2025,
    '2025-05-03',

    'Ravi Menon',
    'Deep Basin Survey',
    'manager1',

    ARRAY['welllog','exploration','survey','basin'],

    'Well log analysis and stratigraphic interpretations.'
),

(
    'TRX-10003',
    'Rig_Inspection_Checklist.docx',
    'stored_10003.docx',
    'D:/vault/TRX-10003.docx',
    256000,
    '.docx',

    'Southern',
    'DOCX',
    'Safety',

    2024,
    '2024-11-11',

    'Karan Verma',
    'Rig Safety Audit',
    'employee1',

    ARRAY['rig','inspection','safety','audit'],

    'Monthly rig inspection compliance checklist.'
),

(
    'TRX-10004',
    'Drilling_Expense_Summary.xlsx',
    'stored_10004.xlsx',
    'D:/vault/TRX-10004.xlsx',
    780000,
    '.xlsx',

    'Central',
    'XLSX',
    'Finance',

    2025,
    '2025-04-18',

    'Neha Kulkarni',
    'Drilling Budget FY25',
    'admin',

    ARRAY['finance','budget','drilling','expenses'],

    'Quarterly drilling expenditure report.'
),

(
    'TRX-10005',
    'Gas_Compressor_Maintenance_Report.pdf',
    'stored_10005.pdf',
    'D:/vault/TRX-10005.pdf',
    920000,
    '.pdf',

    'North Eastern',
    'PDF',
    'Maintenance',

    2023,
    '2023-09-21',

    'Rahul Das',
    'Compressor Overhaul',
    'manager1',

    ARRAY['maintenance','compressor','gas','overhaul'],

    'Detailed maintenance report for compressor systems.'
);


-- =========================================================
-- VERIFY DATA
-- =========================================================

SELECT * FROM public.users;

SELECT * FROM public.indexed_documents;
