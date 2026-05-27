DROP TABLE IF EXISTS indexed_documents;

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE indexed_documents (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),

    file_name TEXT NOT NULL,
    file_path TEXT NOT NULL,

    dynamic_metadata JSONB,

    uploaded_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_indexed_documents_metadata
ON indexed_documents
USING GIN(dynamic_metadata);

CREATE INDEX idx_indexed_documents_file_name
ON indexed_documents(file_name);

CREATE INDEX idx_indexed_documents_file_path
ON indexed_documents(file_path);


ALTER TABLE indexed_documents
ADD COLUMN source_excel_file TEXT;
