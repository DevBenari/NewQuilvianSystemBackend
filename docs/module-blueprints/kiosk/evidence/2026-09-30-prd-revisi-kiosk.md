# Bukti Input — PRD Revisi Module Kiosk & Cek Nomor Rekam Medis

| Field | Nilai |
| --- | --- |
| Sumber | PRD Tim Bisnis, "Revisi Module Kiosk & Fitur Cek Nomor Rekam Medis" |
| Tanggal PRD | 30 September 2026 |
| Status PRD | Draft Requirement |
| Dicatat | 30 September 2026, oleh agent atas permintaan Sukma Giri Pratama |
| Klasifikasi | Bukti requirement bisnis — **bukan** keputusan desain. Item bertanda OPEN belum boleh dipakai sebagai dasar kontrak. |

Dokumen ini merangkum isi PRD per ID requirement supaya setiap artefak blueprint berikutnya dapat merujuk ID yang sama. Kalimat di kolom "Pernyataan" adalah ringkasan setia dari PRD, bukan tafsiran baru.

## 1. Tiga fokus PRD

1. **Cek Nomor Rekam Medis** — gerbang untuk membedakan Pasien Lama dan Pasien Baru, pencarian hanya via No. KTP atau No. HP.
2. **Revisi urutan flow Pasien Lama** — Tujuan Layanan dipindah menjadi Step 3, sesudah Review Data.
3. **Enhancement existing** — Penjamin Utama dan perbaikan dropdown Cek Jadwal Dokter.

## 2. Register requirement

### Existing behaviour (tidak boleh regresi)

| ID | Pernyataan |
| --- | --- |
| KSK-BASE-001 | Nama pasien lebih dari tiga kata sudah didukung dan tidak boleh regresi. |
| KSK-BASE-002 | Bulan pada tanggal lahir tampil sebagai nama pendek tiga huruf (Jan, Feb, Sep). |

### Cek Nomor Rekam Medis

| ID | Pernyataan | Prioritas |
| --- | --- | --- |
| KSK-RM-001 | Menu Cek Nomor Rekam Medis mudah ditemukan dari halaman utama Kiosk. | MUST |
| KSK-RM-002 | Pencarian hanya via No. KTP atau No. HP; tidak via nama, No. RM, email, tanggal lahir, alamat. | MUST |
| KSK-RM-003 | KTP: wajib, angka saja, tanpa huruf/script/HTML, spasi dibuang, 16 digit. Pesan: "Nomor KTP harus terdiri dari 16 digit." | MUST |
| KSK-RM-004 | HP: wajib, format Indonesia (`08…`, `628…`, `+628…`), karakter tak relevan dinormalisasi, backend mencari dengan nomor ternormalisasi. Aturan final mengikuti standar backend. | MUST |
| KSK-RM-005 | Alur: input → validasi → cari → ditemukan / tidak ditemukan. | MUST |
| KSK-RM-006 | Ditemukan: status ditemukan + Kartu Pasien + CTA "Lanjut Pendaftaran Pasien Lama" → flow Pasien Lama. | MUST |
| KSK-RM-007 | Hanya field Kartu Pasien existing; tidak menampilkan alamat lengkap, identitas tambahan, data medis, diagnosis, finansial. | MUST |
| KSK-RM-008 | Tidak ditemukan: "Pasien Belum Terdaftar" + subteks + CTA "Daftar Sebagai Pasien Baru". | MUST |
| KSK-RM-009 | Keputusan FOUND / NOT_FOUND wajib dari backend, bukan cache/data lokal. | MUST |

### Flow Pasien Lama

| ID | Step | Pernyataan |
| --- | --- | --- |
| KSK-OLD-001 | — | Urutan: Identifikasi → Review Data → Tujuan Layanan → Jenis Kunjungan → Pembayaran → Layanan & Dokter → Konfirmasi → Cetak Antrean. |
| KSK-OLD-002 | 1 | Identifikasi; Tujuan Layanan tidak boleh dipilih sebelum Identifikasi selesai. |
| KSK-OLD-003 | 2 | Review Data; Tujuan Layanan tidak lagi sebelum Review Data. |
| KSK-OLD-004 | 3 | Tujuan Layanan. |
| KSK-OLD-005 | 4 | Jenis Kunjungan, mengikuti konfigurasi existing. |
| KSK-OLD-006 | 5 | Pembayaran; pemilihan Penjamin Utama terjadi di sini bila kandidat > 1. |
| KSK-OLD-007 | 6 | Layanan & Dokter (+ jadwal). |
| KSK-OLD-008 | 7 | Konfirmasi: ringkasan pasien, tujuan layanan, jenis kunjungan, pembayaran/penjamin, layanan, dokter, jadwal. |
| KSK-OLD-009 | 8 | Cetak Antrean, tahap terakhir. |
| KSK-OLD-010 | — | Tidak boleh lompat step; setiap tahap menerima konteks valid dari tahap sebelumnya. |

### Penjamin Utama

| ID | Pernyataan |
| --- | --- |
| KSK-GUA-001 | Pasien dengan Asuransi **dan** Penjamin Perusahaan wajib memilih Penjamin Utama. |
| KSK-GUA-002 | Satu kunjungan = satu primary guarantor; relasi pasien ke penjamin lain tidak dihapus. Hanya asuransi → flow existing; hanya perusahaan → flow existing. |

### Cek Jadwal Dokter

| ID | Pernyataan |
| --- | --- |
| KSK-SCH-001 | Dropdown Poliklinik & Spesialis tampil di atas Card Dokter, tidak terpotong, area sentuh cukup, bisa di-scroll. |

### UX, keamanan, privasi, performa

| ID | Pernyataan |
| --- | --- |
| KSK-UX-001 | Loading "Memeriksa Data…", tombol terkunci, duplicate request dicegah. |
| KSK-UX-002 | Error teknis ≠ tidak ditemukan. Pesan: "Data pasien belum dapat diperiksa. Silakan coba kembali." + CTA "Coba Lagi". |
| KSK-UX-003 | Timeout/network error tidak boleh dianggap pasien baru. |
| SEC-KSK-001 | Validasi ulang di backend. |
| SEC-KSK-002 | Cegah XSS, HTML/script injection, control characters. |
| SEC-KSK-003 | Query terparameterisasi / ORM. |
| SEC-KSK-004 | Respons lookup minimal. |
| SEC-KSK-005 | Log tidak menyimpan KTP/HP lengkap. |
| SEC-KSK-006 | Rate limiting lookup (anti-enumeration). |
| SEC-KSK-007 | Data pasien dibersihkan saat selesai, batal, timeout, kembali ke Home. |
| PRIV-1..5 | Tidak ada KTP/HP di URL atau browser storage; tidak ada info kesehatan di lookup; hanya field Kartu Pasien yang disetujui. |
| PERF | Satu aksi = satu request aktif; request lama boleh dibatalkan. |

### Kontrak backend konseptual (bukan kontrak final)

`POST /kiosk/patient-lookup` dengan `{ searchType: "KTP" | "PHONE", value }` → `{ found, patient: { patientId, medicalRecordNumber, patientCard }, nextAction }`. PRD menyatakan naming final mengikuti arsitektur backend existing.

### Out of scope

Struktur No. RM, merger rekam medis, master pasien/asuransi/perusahaan, Dukcapil, OCR KTP, biometrik, OTP HP, struktur Kartu Pasien.

## 3. Keputusan terbuka dari PRD

| ID | Pertanyaan | Status |
| --- | --- | --- |
| DEC-KSK-001 | Satu No. HP dipakai beberapa pasien — bagaimana perilakunya? | OPEN |
| DEC-KSK-002 | Definisi field Kartu Pasien. | Existing behaviour (ikuti Kartu Pasien existing) |
| DEC-KSK-003 | Penjamin utama hanya untuk kunjungan ini atau mengubah default pasien? | OPEN |
| DEC-KSK-004 | Bolehkah primary guarantor diganti setelah kunjungan/billing/pelayanan dimulai? | OPEN |
| DEC-KSK-005 | Lama inactivity timeout Kiosk. | OPEN |

## 4. Temuan audit awal (read-only, belum capability map resmi)

Dicatat pada SHA backend `419b910f` dan frontend `4ec51b0b`. Capability map resmi disusun oleh `trace-existing-capabilities`.

| Area | Temuan | Rujukan |
| --- | --- | --- |
| Tile Cek No. RM | Ada di home tetapi hanya menampilkan notice "akan dilanjutkan pada tahap berikutnya", tanpa route. | FE `src/components/view/kiosk/kiosk-home-view.jsx` item `medicalRecordCheck` |
| Lookup KTP/HP | Tidak ada endpoint khusus. Yang ada `GET .../patients/kiosk?search=` (free-text, paginasi, lewat `GetPatients`). | BE `Areas/HealthServices/PatientManagement/MasterData/Controllers/PatientController.cs#GetPatientsForKiosk` |
| Kolom pasien | `MstPatient.IdentityNumber` dan `MstPatient.PhoneNumber` tersedia. | BE `.../Models/MstPatient.cs` |
| Rate limiting | Tidak ada `AddRateLimiter` / `EnableRateLimiting` di backend. | BE seluruh repo |
| Urutan step | Sekarang `serviceTarget → find → review → type → payment → service → confirm → ticket`. | FE `src/lib/hooks/kiosk/registration/use-kiosk-old-patient-registration.jsx#OLD_PATIENT_STEP_ITEMS` |
| Alasan urutan lama | Tujuan Layanan sengaja sebelum Identifikasi karena `targetService` + `hasPhysicianRequest` hanya bisa ditulis sekali saat sesi kiosk dibentuk (scan di Identifikasi). | FE `src/lib/hooks/kiosk/registration/kiosk-service-target-rules.js`; blueprint `laboratorium` FE-LAB-13 / AC-93 |
| Penjamin Utama | Step Pembayaran sudah punya "Jadikan Utama" yang memanggil `PATCH .../patient-insurances/kiosk/{id}/primary` dan `.../patient-company-guarantors/kiosk/{id}/primary` — mengubah primary **level pasien**. | FE `kiosk-old-patient-step-payment.jsx`; BE `PatientCompanyGuarantorController.cs` baris 555 |
| Satu penanggung | Keputusan `RJ-BIL-DEC-015`: satu kunjungan tepat satu penanggung — selaras dengan KSK-GUA-002. | blueprint `rawat-jalan/00-interview-decisions.md` |
| Dropdown jadwal dokter | Memakai `ResourceFilterSelect` tanpa portal. Dugaan akar masalah: stacking context card. Perlu dikonfirmasi saat trace. | FE `src/components/view/kiosk/registration/doctor-schedule/kiosk-doctor-schedule-view.jsx` |
