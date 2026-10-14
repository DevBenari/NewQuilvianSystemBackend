# Roadmap backend admisi bayi

Disetujui pengguna 14 Oktober 2026 bersama plan repair 016. Kontrak terkunci: RWI-BAYI-016-1.0. ID global dipindai sebelum alokasi: BE tertinggi 230, FE tertinggi 244.

## ✅ BE-RWI-231 — Konteks dan kelayakan admisi bayi

Status: ✅ SELESAI SOURCE - 2026-10-14. Build final 0 error/279 warning dengan flag pengguna; migration/Designer/snapshot disiapkan tanpa DB update. Runtime PostgreSQL belum diuji dan tidak termasuk penerapan source task. [Laporan](../task/report/backend/BE-RWI-231.md).

Target: NewQuilvianSystemBackend, branch MHamzah/upstream origin/MHamzah. Area HealthServices, module InPatientManagement, prefix Inp ACTIVE. Applicability TOUCHED LEGACY dan NEW CODE service/DTO; QBE-SVC-001, API-001, VAL-001, CFG-001, MOD-001, PERM-001, TXN-001, LOG-001. Tidak ada entity/prefix baru.

Acceptance criteria:
1. Metadata newbornAdmission tersimpan dan terbaca dengan validasi kategori master, sumber, perawatan, penjaminan keluarga dan rencana deposit.
2. Hubungan pasien ibu/episode konsisten; referensi historis tidak terputus; ibu tidak dibuat fiktif.
3. Kelayakan bayi/dewasa dua arah, rawat gabung kamar ibu, intensif dan isolasi ditegakkan pada pencarian dan transaksi bed.
4. Pencarian sebelum EpisodeId memakai konteks pasien; setelah EpisodeId memakai metadata tersimpan.
5. File migration, Designer dan snapshot tersedia tanpa menerapkan database; build aplikasi dijalankan dan hasilnya dicatat.

Dependency: kontrak RWI-BAYI-016-1.0.

~~~text
✅ BE-RWI-231
└── 🟡 FE-RWI-245
~~~

Laporan: ../task/report/backend/BE-RWI-231.md.
