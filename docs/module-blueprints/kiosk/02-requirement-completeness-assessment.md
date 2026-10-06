# Kiosk — Penilaian Kelengkapan Requirement

| Field | Nilai |
|---|---|
| Assessment ID | `KSK-RCG-001` |
| Revision | `1` |
| Blueprint ID | `KSK-BP-001` |
| Status | `draft` |
| **Kesiapan modul** | **`READY_FOR_DOMAIN_DESIGN`** — 9 dari 9 slice boleh maju ke desain. Dua dependency lintas blueprint (`KSK-OQ-004`, `KSK-OQ-005`) baru memblokir **implementasi** slice terkait, bukan desain. |
| Masukan | `00-interview-decisions.md` r2 (`KSK-DEC-001..019`); `01-existing-capability-map.md` r1 final; `evidence/2026-09-30-prd-revisi-kiosk.md` |
| SHA backend | `419b910f` — branch `sukmagp` |
| SHA frontend | `4ec51b0b` |
| Tanggal penilaian | 30 September 2026 |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_NOT_RUN` — `hospital-domain-architect` tidak dipakai (alasan §9) |
| `indonesia-hospital-domain-reference` | Tidak dipakai. Bukti berasal dari PRD Tim Bisnis, keputusan pemilik, dan audit source; tidak ada gap yang butuh baseline rujukan |
| Task mode | `MODULE BLUEPRINT MODE` — read-only terhadap source aplikasi |

> **Cara membaca dokumen ini.** Dokumen ini hanya menjawab satu pertanyaan: *apakah requirement Kiosk sudah cukup lengkap untuk mulai dirancang?* Dokumen ini tidak merancang tabel, endpoint, atau layar, dan tidak menjawab keputusan bisnis atas nama siapa pun.
>
> | Status bukti | Artinya |
> |---|---|
> | `CONFIRMED` | Didukung bukti berwenang (PRD, keputusan pemilik, atau source untuk kondisi existing) |
> | `PROPOSED` | Usulan wajar yang **belum** dikonfirmasi pemilik |
> | `MISSING` | Tidak ada dalam bukti |
> | `CONFLICT` | Dua sumber bertentangan tanpa penyelesaian |
>
> | Dampak gap | Artinya |
> |---|---|
> | `BLOCKING` | Desain slice terdampak harus berhenti |
> | `NON_BLOCKING_STANDARD` | Usulan standar boleh dipakai asal tetap terlihat |
> | `CONFIGURABLE_DEFAULT` | Wajar berbeda antar RS; aman dijadikan pengaturan |

---

## 1. Scope penilaian

Modul **Kiosk** dengan bentuk blueprint `SINGLE` (`KSK-DEC-004`). Empat rumpun dipecah menjadi sembilan slice proses: potongan terkecil yang masih bermakna dan bisa dinilai sendiri.

| Slice | Nama | Rumpun | Requirement PRD | Keputusan utama |
|---|---|---|---|---|
| `S1` | Lookup No. RM (KTP/HP) dan hasilnya | Lookup | KSK-RM-001..009, KSK-UX-001..003, SEC-KSK-001..004 | `KSK-DEC-006/007/017/018/019` |
| `S2` | Rate limit lookup | Lookup | SEC-KSK-006 | `KSK-DEC-011` |
| `S3` | Handoff Cek No. RM → Pasien Lama / Pasien Baru | Lookup + Flow | KSK-RM-006/008, AC-OLD-001 | `KSK-DEC-012` |
| `S4` | Urutan 8 step dan waktu pembentukan sesi kiosk | Flow | KSK-OLD-001..010 | `KSK-DEC-002/014` |
| `S5` | Perbaikan privasi Step 1 Identifikasi | Flow | PRIV-2/3, SEC-KSK-005 | `KSK-DEC-016` |
| `S6` | Pembersihan sesi dan inactivity timeout | Flow | SEC-KSK-007 | `KSK-DEC-010` |
| `S7` | Penjamin Utama per kunjungan (termasuk Perusahaan) | Penjamin | KSK-GUA-001/002, KSK-OLD-006 | `KSK-DEC-008/009/013` |
| `S8` | Dropdown Poliklinik & Spesialis di Jadwal Dokter | Jadwal | KSK-SCH-001 | `KSK-UI-005` |
| `S9` | Regresi nama > 3 kata dan format tanggal lahir | Flow | KSK-BASE-001/002 | `KSK-DEC-015` |

## 2. Bukti yang dipakai

| Urutan wewenang | Sumber | Dipakai untuk |
|---|---|---|
| 1 — requirement eksplisit | PRD Tim Bisnis 30 Sep 2026 (register di `evidence/`) | Apa yang harus dibangun |
| 1 — requirement eksplisit | Keputusan Sukma Giri Pratama atas nama Tim Bisnis (`KSK-DEC-005`) | Penutupan OPEN PRD dan conflict |
| 1 — requirement eksplisit | Persetujuan Muhammad Hamzah atas `RWI-ENC-PAYER-001` (diteruskan Sukma, `KSK-DEC-013`) | Kiosk menerima Penjamin Perusahaan |
| 3 — keputusan tercatat | `RJ-BIL-DEC-015` (satu kunjungan satu penanggung), keputusan blueprint Laboratorium `FE-LAB-13` / `AC-93` | Batas lintas modul |
| 6 — implementasi V2 | Capability map r1 dan query baca-saja `QuilvianNewDevSukma` | Apa yang ada hari ini |

## 3. Temuan per slice

Kolom dimensi memakai nomor pada kontrak penilaian (01 Tujuan … 18 Pelaporan). Hanya dimensi yang material yang diuraikan; dimensi kondisional yang tidak berlaku disebut alasannya pada §4.

### S1 — Lookup No. RM

| Dim | Temuan | Status | Bukti |
|---|---|---|---|
| 01 | Membedakan pasien lama/baru dan mencegah pasien ganda | `CONFIRMED` | PRD §1–2 |
| 02 | Pasien di Kiosk; akun perangkat Kiosk sebagai identitas teknis | `CONFIRMED` | `00` §3 |
| 03 | Perangkat login dengan akun Kiosk (`KioskRead`) | `CONFIRMED` | `KSK-CAP-006` |
| 04 | Pilih KTP/HP → isi → validasi → cek → hasil | `CONFIRMED` | KSK-RM-005, `00` §5 |
| 05 | Tepat satu / tidak ada / ganda / status non-aktif / gagal teknis / 429 | `CONFIRMED` | `KSK-DEC-006/007/011/017`, KSK-UX-002/003 |
| 06 | Masukan: jenis + nilai. Keluaran FOUND: `patientId` + field Kartu Pasien | `CONFIRMED` | `KSK-FACT-005`, `KSK-DEC-012` |
| 07 | KTP tepat 16 digit; HP Indonesia dinormalisasi ke `62…` 9–15 digit, dicocokkan di kedua sisi; hanya pasien Aktif; Merged diikuti | `CONFIRMED` | `KSK-DEC-017/018/019` |
| 08 | Tidak ada status persisten; keadaan layar IDLE → CHECKING → FOUND/NOT_FOUND/MULTIPLE/CONTACT_STAFF/ERROR | `CONFIRMED` | `00` §5 |
| 09 | Hanya `KioskRead` | `CONFIRMED` | `KSK-CAP-006` |
| 10 | Membaca master pasien (`Pat`) tanpa mengubahnya | `CONFIRMED` | `KSK-DEC-003` |
| 12 | Kartu tampil + CTA, atau CTA Pasien Baru, atau arahan petugas | `CONFIRMED` | PRD §13, §15 |
| 13 | Tidak ada yang dibatalkan; lookup tidak menulis data | `CONFIRMED` | — |
| 14 | Jejak lookup: perangkat, waktu, jenis pencarian, jenis hasil, **nilai disamarkan** (4 digit terakhir) di log terstruktur, tanpa tabel baru | `PROPOSED` | Usulan dari SEC-KSK-005 dan `KSK-AC-008` |
| 17 | Salah pasien dicegah oleh pencocokan persis, aturan tepat-satu, dan step Review | `CONFIRMED` | `KSK-INV-003`, KSK-OLD-003 |

### S2 — Rate limit

| Dim | Temuan | Status | Bukti |
|---|---|---|---|
| 07 | 10 permintaan/menit per akun perangkat; `429` ≠ tidak ditemukan | `CONFIRMED` | `KSK-DEC-011` |
| 07 | Kuota yang sama berlaku untuk lookup dari Step 1 (`KSK-DEC-016`) karena endpointnya sama | `PROPOSED` | Konsekuensi `KSK-DEC-016` |
| 10 | Fitur rate limit pertama di backend; berlaku hanya untuk endpoint lookup, tidak menyentuh endpoint lain | `PROPOSED` | `KSK-CAP-007` |
| 11 | Klaim identitas perangkat untuk partisi (`KSK-UNK-004`) | `MISSING` (teknis) | Diselesaikan dengan trace saat desain |

### S3 — Handoff

| Dim | Temuan | Status | Bukti |
|---|---|---|---|
| 04 | FOUND → Step 1 terisi → Lanjut ke Review; NOT_FOUND → Pendaftaran Pasien Baru | `CONFIRMED` | `KSK-DEC-012`, KSK-RM-008 |
| 06 | Handoff hanya membawa `patientId` di memori, bukan URL | `CONFIRMED` | `KSK-DEC-012` |
| 06 | KTP/HP yang diketik **tidak** diteruskan ke form Pasien Baru (alur Pasien Baru di luar scope; tidak ada data yang tersisa di memori setelah pindah) | `PROPOSED` | Konsekuensi `KSK-DEC-003` + PRIV-1 |

### S4 — Urutan step dan sesi kiosk

| Dim | Temuan | Status | Bukti |
|---|---|---|---|
| 04 | Identifikasi → Review → Tujuan Layanan → Jenis Kunjungan → Pembayaran → Layanan & Dokter → Konfirmasi → Cetak Antrean | `CONFIRMED` | KSK-OLD-001 |
| 05 | Tujuan **Laboratorium** berhenti setelah Step 3 (Identifikasi → Review → Tujuan Layanan → Selesai/handoff lab), sesuai keputusan blueprint Laboratorium | `CONFIRMED` | `KSK-DEC-002`, `00` §5; blueprint `laboratorium` FE-LAB-13 |
| 07 | Tidak boleh lompat step; Tujuan Layanan tidak tampil sebelum Review | `CONFIRMED` | KSK-OLD-010, `KSK-INV-004` |
| 08 | Sesi kiosk dibentuk tepat sekali di Step 3 untuk semua tujuan | `CONFIRMED` | `KSK-DEC-014`, `KSK-INV-008` |
| 08 | Cara Step 1 mengenali kartu yang hanya memuat nomor asuransi/member tanpa membentuk sesi | `MISSING` (teknis) | `KSK-DEC-014` menyerahkannya ke desain |
| 10 | Blueprint Laboratorium harus diamandemen (`KSK-OQ-004`) | `CONFIRMED` sebagai dependency | — |
| 12 | Kunjungan terbentuk + antrean tercetak, atau handoff lab | `CONFIRMED` | KSK-OLD-009 |

### S5 — Privasi Step 1

| Dim | Temuan | Status | Bukti |
|---|---|---|---|
| 07 | Input berbentuk KTP 16 digit/HP → endpoint lookup POST; nama/No. RM tetap jalur lama; `console.log` data pasien dihapus | `CONFIRMED` | `KSK-DEC-016` |
| 07 | Cara membedakan "input berbentuk HP" dari "No. RM" yang juga berupa angka | `MISSING` (teknis) | Diselesaikan saat desain dari format `MedicalRecordNumber` existing |

### S6 — Pembersihan sesi

| Dim | Temuan | Status | Bukti |
|---|---|---|---|
| 07 | 120 detik tanpa sentuhan + peringatan 15 detik → bersihkan → Home; berlaku di Cek No. RM dan seluruh step Pasien Lama | `CONFIRMED` | `KSK-DEC-010` |
| 05 | Timeout **saat request sedang berjalan** (misalnya Konfirmasi sedang membuat kunjungan): tunggu request selesai dulu, baru hitung ulang | `PROPOSED` | Usulan standar untuk mencegah kunjungan setengah jadi |
| 07 | Nilai 120/15 detik dapat diatur per perangkat | `PROPOSED` | Keputusan hanya menetapkan angka |

### S7 — Penjamin Utama

| Dim | Temuan | Status | Bukti |
|---|---|---|---|
| 01 | Satu penanggung untuk kunjungan; relasi pasien lain tetap | `CONFIRMED` | KSK-GUA-002, `RJ-BIL-DEC-015` |
| 04 | Kondisi A (asuransi saja) dan B (perusahaan saja) → alur existing; Kondisi C → wajib pilih satu | `CONFIRMED` | PRD §29 |
| 05 | Pasien dengan >1 asuransi (tanpa perusahaan) → alur existing memilih satu | `CONFIRMED` | PRD §29 Kondisi A |
| 06 | Kunjungan membawa `paymentType` + `patientInsuranceId` **atau** `patientCompanyGuarantorId` | `CONFIRMED` | `KSK-FACT-004`, `KSK-DEC-013` |
| 07 | Validasi kelayakan perusahaan di route kiosk sama dengan route admin (aktif, eligible, masa berlaku) | `CONFIRMED` | `KSK-DEC-013` |
| 07 | Kondisi C tidak memilih apa pun secara otomatis (termasuk penjamin yang ditandai "utama" di data pasien); pasien harus memilih sendiri | `PROPOSED` | Tafsiran "wajib menentukan" KSK-GUA-002 |
| 09 | Kiosk tidak lagi memakai `PATCH …/primary` (default pasien) | `CONFIRMED` | `KSK-DEC-008` |
| 09 | Nasib route `PATCH …/kiosk/{id}/primary`: dibiarkan (tidak dipanggil Kiosk) atau ditutup | `PROPOSED` | `KSK-CAP-033` — dibiarkan, agar tidak mematahkan konsumen lain yang belum diaudit |
| 10 | Amendment `RWI-ENC-PAYER-001` v1.1.0 tercatat di blueprint rawat-inap (`KSK-OQ-005`) | `CONFIRMED` sebagai dependency | — |
| 13 | Setelah Konfirmasi tidak bisa diganti dari Kiosk | `CONFIRMED` | `KSK-DEC-009` |
| 16 | Kunjungan Penjamin Perusahaan dari Kiosk diproses billing-kasir dengan jalur yang sama seperti dari route admin; tidak ada tarif atau aturan tagihan baru | `CONFIRMED` | `RWI-ENC-PAYER-001`, `KSK-DEC-013` |

### S8 — Dropdown Jadwal Dokter

| Dim | Temuan | Status | Bukti |
|---|---|---|---|
| 07 | Menu di atas card, tidak terpotong, bisa di-scroll, area sentuh cukup | `CONFIRMED` | PRD §31 |
| 10 | Komponen dasar dipakai 33 berkas; perbaikan tidak boleh mengubah layar lain | `CONFIRMED` | `KSK-CAP-040`, `KSK-UI-005` |

### S9 — Regresi

| Dim | Temuan | Status | Bukti |
|---|---|---|---|
| 07 | Nama > 3 kata utuh (tidak dipotong per kata) | `CONFIRMED` | KSK-BASE-001, `KSK-CAP-026` |
| 07 | Tanggal lahir bulan pendek di Review & Konfirmasi | `CONFIRMED` | `KSK-DEC-015` |

## 4. Dimensi kondisional

| Dim | Berlaku? | Alasan |
|---|---|---|
| 15 Notifikasi | Tidak material | Tidak ada pihak yang harus diberi tahu. Arahan "hubungi petugas" hanya tampil di layar, tanpa notifikasi ke petugas; PRD tidak memintanya. |
| 16 Billing | Material hanya di `S7` | Dinilai di §3 S7. Slice lain tidak membuat atau mengubah konsekuensi finansial. |
| 17 Keselamatan klinis | Material di `S1`, `S3`, `S4` (identifikasi pasien) | Dinilai di §3. Risiko salah pasien ditutup oleh aturan tepat-satu, pencocokan persis, Review wajib, dan satu sesi per kunjungan. |
| 18 Pelaporan | Tidak material | Kunjungan Kiosk sudah bertanda `IsFromKiosk` / `RegistrationSource`. PRD tidak meminta laporan lookup. |

## 5. Daftar gap dan dampaknya

| ID | Slice | Gap | Status | Dampak | Alasan dampak |
|---|---|---|---|---|---|
| `KSK-GAP-001` | S1 | Jejak lookup di log terstruktur dengan nilai disamarkan, tanpa tabel | `PROPOSED` | `NON_BLOCKING_STANDARD` | Tidak mengubah model persisten; memenuhi SEC-KSK-005 |
| `KSK-GAP-002` | S2 | Kuota rate limit dipakai bersama oleh Cek No. RM dan Step 1 | `PROPOSED` | `NON_BLOCKING_STANDARD` | Endpoint sama; memisahkan kuota justru melemahkan perlindungan enumerasi |
| `KSK-GAP-003` | S2 | Rate limit hanya terpasang pada endpoint lookup | `PROPOSED` | `NON_BLOCKING_STANDARD` | Mencegah dampak lintas modul |
| `KSK-GAP-004` | S2 | Klaim identitas perangkat untuk partisi | `MISSING` | `NON_BLOCKING_STANDARD` | Pertanyaan teknis, dijawab dari source saat desain |
| `KSK-GAP-005` | S3 | KTP/HP tidak dibawa ke Pasien Baru | `PROPOSED` | `NON_BLOCKING_STANDARD` | Paling aman untuk privasi; alur Pasien Baru di luar scope |
| `KSK-GAP-006` | S4 | Pengenalan kartu asuransi/member di Step 1 tanpa sesi | `MISSING` | `NON_BLOCKING_STANDARD` | Aturan bisnis sudah dikunci `KSK-DEC-014`; yang tersisa hanya caranya |
| `KSK-GAP-007` | S5 | Pembeda input HP vs No. RM | `MISSING` | `NON_BLOCKING_STANDARD` | Teknis; format RM existing dapat ditelusuri |
| `KSK-GAP-008` | S6 | Timeout ditunda selama request berjalan | `PROPOSED` | `NON_BLOCKING_STANDARD` | Mencegah kunjungan setengah jadi; tidak mengubah keputusan 120 detik |
| `KSK-GAP-009` | S6 | Angka timeout dapat diatur | `PROPOSED` | `CONFIGURABLE_DEFAULT` | Wajar berbeda antar lokasi Kiosk; default tetap 120/15 detik |
| `KSK-GAP-010` | S7 | Kondisi C tanpa pilihan otomatis | `PROPOSED` | `NON_BLOCKING_STANDARD` | Sesuai kata "wajib menentukan"; tidak mengubah model |
| `KSK-GAP-011` | S7 | Route `PATCH …/kiosk/{id}/primary` dibiarkan, tidak dipanggil Kiosk | `PROPOSED` | `NON_BLOCKING_STANDARD` | Menutup route berisiko mematahkan konsumen yang belum diaudit; `KSK-DEC-008` tetap terpenuhi |

Tidak ada gap berstatus `CONFLICT`. Tidak ada gap `BLOCKING`.

## 6. Decision Log

Tidak ada Decision ID baru yang dibuat gate ini, karena tidak ada ambiguitas pemblokir yang bergantung pemilik. Keputusan yang dirujuk: `KSK-DEC-001..019` (seluruhnya `approved`).

Butir `PROPOSED` pada §5 tetap usulan. Desain wajib menampilkannya sebagai usulan standar, dan pemilik boleh membatalkannya lewat amendment pass `grill-me` tanpa menghentikan desain.

## 7. Dependency di luar wewenang blueprint ini

| ID | Dependency | Pemilik | Memblokir | Tidak memblokir |
|---|---|---|---|---|
| `KSK-OQ-004` | Amendment blueprint Laboratorium `FE-LAB-13` / `AC-93` (urutan step dan waktu sesi) | Sukma | Build FE slice `S4` | Desain seluruh slice; build `S1–S3`, `S5–S9` |
| `KSK-OQ-005` | Catatan amendment `RWI-ENC-PAYER-001` v1.1.0 di blueprint rawat-inap | Sukma / Muhammad Hamzah | Build BE slice `S7` (route kiosk menerima Penjamin Perusahaan) | Desain seluruh slice; build FE `S7` bisa disiapkan berdasarkan kontrak target |

## 8. Kesiapan per slice

| Slice | Kesiapan | Catatan |
|---|---|---|
| `S1` Lookup No. RM | `READY_FOR_DOMAIN_DESIGN` | — |
| `S2` Rate limit | `READY_FOR_DOMAIN_DESIGN` | `KSK-GAP-004` diselesaikan saat desain |
| `S3` Handoff | `READY_FOR_DOMAIN_DESIGN` | Bergantung pada kontrak `S1` |
| `S4` Urutan step & sesi | `READY_FOR_DOMAIN_DESIGN` | Implementasi menunggu `KSK-OQ-004` |
| `S5` Privasi Step 1 | `READY_FOR_DOMAIN_DESIGN` | Bergantung pada endpoint `S1` |
| `S6` Pembersihan sesi | `READY_FOR_DOMAIN_DESIGN` | — |
| `S7` Penjamin Utama | `READY_FOR_DOMAIN_DESIGN` | Implementasi BE menunggu `KSK-OQ-005` |
| `S8` Dropdown Jadwal Dokter | `READY_FOR_DOMAIN_DESIGN` | Mandiri, FE saja |
| `S9` Regresi | `READY_FOR_DOMAIN_DESIGN` | — |

**Yang boleh berjalan:** seluruh sembilan slice ke `design-business-module`.

**Yang harus berhenti:** tidak ada pada tahap desain. Pada tahap implementasi, build FE `S4` berhenti sampai `KSK-OQ-004` selesai, dan build BE `S7` berhenti sampai `KSK-OQ-005` selesai.

**Keputusan pemilik yang dibutuhkan:** tidak ada yang memblokir. Opsional: konfirmasi atau tolak butir `PROPOSED` pada §5.

## 9. Handoff

| Field | Nilai |
|---|---|
| Skill berikutnya | `design-business-module` |
| Alasan melewati `hospital-domain-architect` | Semua slice berada di satu bounded context (`RegistrationManagement` / Kiosk, `KSK-DEC-004`). Tidak ada entity atau master data baru yang dimiliki bersama, dan lookup hanya membaca `MstPatient`. Dampak billing `S7` sudah ditetapkan oleh `RJ-BIL-DEC-015` dan `RWI-ENC-PAYER-001` tanpa aturan tarif baru. Keselamatan klinis terbatas pada identifikasi pasien dan sudah dikunci oleh invariant `KSK-INV-003/008`. |
| Masukan yang diteruskan | `00-interview-decisions.md` r2; `01-existing-capability-map.md` r1; `KSK-RCG-001` r1; SHA BE `419b910f`, FE `4ec51b0b` |
| Keluaran hilir yang diharapkan | Blueprint lengkap sesuai kontrak keluaran `design-business-module`: arsitektur BE/FE, kontrak API lookup, matriks validasi/state/permission, flowchart, dan acceptance matrix, dengan butir `PROPOSED` §5 ditandai sebagai usulan standar |
