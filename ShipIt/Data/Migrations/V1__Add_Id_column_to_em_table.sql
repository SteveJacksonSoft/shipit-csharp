ALTER TABLE em DROP CONSTRAINT em_pkey;
ALTER TABLE em ADD COLUMN id BIGSERIAL PRIMARY KEY;