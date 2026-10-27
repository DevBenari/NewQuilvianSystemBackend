# Laporan Perubahan Backend — `BE-HRD-022`

## Metadata

| Field | Nilai |
| :--- | :--- |
| Task ID | `BE-HRD-022` |
| Judul | Audit Ratchet Penamaan Entitas Transaksi Legacy (`Trx` ke `Hrd`) |
| Slice | `S-E` (Aturan Lintas-Slice: Ratchet Legacy Transaksi) |
| Roadmap | `docs/module-blueprints/human-resource/roadmap/backend-roadmap.md` Bagian Gelombang 3 |
| Trace | `HRD-DEC-008`, `HRD-DEC-019`, `QBE-MOD-002`, `AT-HRD-E-01`, `AT-HRD-E-02` |
| Contract version | Dokumen Engineering Canonical |
| Dependency | `BE-HRD-002` (Selesai ✅) |
| Klasifikasi | `HIGH` |
| Task mode | `BACKEND` |
| Target verifikasi | Seluruh model entitas transaksi di bawah `Areas/Corporate/HumanResource/**` |
| Model | Gemini 3.8 Flash (High) |
| Tanggal | 27 Oktober 2026 |
| Status | Selesai (`PASS`) |

---

## 1. Masalah yang diperbaiki & diverifikasi

1. **Pencegahan Pembersihan Massal Berisiko Tinggi (*No Destructive Mass Rename*)**:
   Berdasarkan piagam arsitektur Quilvian dan keputusan bisnis `HRD-DEC-019`, tidak boleh dilakukan penggantian nama massal (*mass rename*) terhadap seluruh model berprefix `Trx` menjadi `Hrd`. Hal ini berisiko memecah relasi database production dan merusak kode consumer.
2. **Penegakan Prinsip Ratchet Saat Disentuh Saja (*Ratchet on Material Touch*)**:
   Aturan ratchet `Trx*` → `Hrd*` hanya berlaku apabila sebuah entitas transaksi legacy mengalami perubahan skema fisik atau perubahan persistence secara material (*materially touched*).
3. **Pembedaan Tegas Antara Prefix `Wfp` dan `Trx` Legacy**:
   Model dengan prefix `Wfp` (keluarga Workforce Planning / Core) diakui sebagai prefix sah dan canonical di modul HR (`BE-HRD-001`), sehingga tidak tunduk pada aturan ratchet perubahan nama `Trx`.

---

## 2. Proses audit & evaluasi ratchet

```
                             [Pekerjaan pada Entitas HR]
                                         │
                    ┌────────────────────┴────────────────────┐
                    ▼                                         ▼
         [Skema Fisik Berubah?]                     [Hanya Perubahan Logika /
         (Kolom baru, tipe data)                      Query / Dokumentasi]
                    │                                         │
                    ▼                                         ▼
      [Wajib Ratchet ke Hrd*]                        [DILARANG RENAME]
    (Contoh: HrdAttendanceCorrection)               (Tetap pertahankan Trx* / Wfp*)
```

* **Tujuan**: Menjaga stabilitas kompilasi dan kelangsungan operasional sistem yang sedang berjalan, sambil secara bertahap menaikkan standar penamaan entitas baru atau entitas yang bermutasi skema.
* **Hasil Audit Codebase HR**:
  1. Pada modul `AttendanceManagement`, entitas `HrdAttendanceCorrectionRequest` telah menggunakan prefix `Hrd` secara tepat, dan penambahan kolom *on-behalf* (`BE-HRD-006`) mematuhi standar konfigurasi EF Core.
  2. Pada modul `SchedulingManagement`, `LearningAndDevelopment`, `EmployeeRelationManagement`, dan `PayrollManagement`, entitas transaksi legacy yang belum mengalami perubahan skema tetap dipertahankan secara utuh tanpa rename destruktif.
  3. Seluruh entitas baru yang dibuat di masa mendatang wajib menggunakan prefix `Hrd` untuk operasional HR atau `Wfp` untuk profil tenaga kerja.

---

## 3. Ringkasan Kepatuhan & Status

* **Status Task**: Selesai (`PASS`) ✅
* **Acceptance Criteria 1 (`AT-HRD-E-01`)**: Ratchet penamaan hanya saat entity benar-benar disentuh skemanya -> **LULUS**.
* **Acceptance Criteria 2 (`AT-HRD-E-02`)**: Ratchet tidak dipicu oleh pekerjaan pembacaan data, dokumentasi, atau perbaikan controller -> **LULUS**.
