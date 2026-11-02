# Laporan Perubahan Backend — `BE-MCU-035`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-MCU-035` |
| Judul | Privasi log dan registri izin |
| Slice | `MVP-3` — `EPIC MCU-06` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian *`BE-MCU-035`* |
| Trace | `NFR-006`; `MCU-DEC-010`; `AT-MCU-45`; `GAP-MCU-05` (daftar tindakan ubah) |
| Contract version | `MCU-PERM-v1`; `02-backend-architecture.md` §11 — `approved` 2026-10-21 |
| Dependency | `BE-MCU-034` ✅ |
| Klasifikasi | `MEDIUM` — skor 7: berkas diperiksa 9–20 (1), berkas diubah ≤ 3 (0, hanya uji sementara + dokumen), logika uji sedang (1), memakai kontrak yang sudah ada (1), persistence yang ada (1), keamanan/privasi inti (2), workflow luas — seluruh alur MCU (1) |
| Task mode | `BACKEND` |
| Target tulis | Uji sementara di `Tests/` (dihapus sesudahnya); laporan ini; tanda roadmap/traceability. Perbaikan teks log **tidak diperlukan** — tidak ada kebocoran |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `cee59643` (branch `yoga`) — tidak bergeser |
| Tanggal | 2026-10-23 |
| Status | Selesai — `AT-MCU-45` lulus, validator registri izin lulus untuk seluruh kunci `MCU-PERM-v1` rilis MVP |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `MedicalCheckupManagement / Medical Check-Up`, prefix `Mcu`, registry `ACTIVE` |
| Keberlakuan | `NOT APPLICABLE` untuk source — task ini hanya uji dan dokumen; tidak ada berkas aplikasi yang berubah |
| QBE yang berlaku | `QBE-LOG-001` (diuji), `QBE-PERM-001` (diuji lewat `PermissionRegistryValidator`), `QBE-AUD-001` (audit terpisah dari log aplikasi — diperhatikan) |
| Checker QBE | `NOT APPLICABLE` — tidak ada berkas aplikasi yang berubah |

---

## 1. Masalah yang diperbaiki

Tiap task MCU menjaga kalimat log-nya sendiri, tetapi belum ada satu uji yang membuktikan bahwa **seluruh** tindakan
ubah MCU — termasuk log modul mitra yang ikut terpanggil (Lab, Rad, Billing, Registrasi) — tidak pernah menulis NIK,
nilai isian, isi resume, atau tujuan penyerahan. Satu kalimat log yang ceroboh cukup untuk membocorkan
*HBsAg reaktif* atau nomor WhatsApp peserta ke sistem log (Loki) yang dibaca banyak orang. Registri izin MCU juga
belum diperiksa sebagai satu kesatuan.

---

## 2. Proses bisnis yang diuji

Satu alur lengkap berjalan lewat **controller sungguhan** (tempat kalimat log disusun), dengan penangkap log yang
merekam setiap pesan dan setiap isi scope dari logger mana pun:

1. Booking identitas sementara (nama, NIK, telepon) → jadwal ulang → batal sebelum datang.
2. Booking Budi → check-in → jadwal ulang komponen → batal penuh (void baris paket).
3. Walk-in Siti (PT Contoh) → mulai pemeriksaan → muat ulang → tambah pemeriksaan di luar paket → batal komponen →
   isi hasil Audiometri (nilai `77.7`, kesimpulan) → resume (kondisi umum, temuan *HBsAg reaktif*, rekomendasi, rekomendasi
   kerja) → approve → kembalikan → approve → verifikasi → catat dan cabut persetujuan pelepasan → serahkan lewat WhatsApp
   dan email → buka ulang → catat pemberitahuan koreksi → ganti dokter.

Ke-22 endpoint ubah yang menyentuh data peserta (grup Order, Check In, Examination, Resume, Delivery) terpanggil.
Sembilan endpoint master (paket, jenis pemeriksaan, kategori kelayakan, jadwal) tidak memuat data peserta dan tidak
ikut dijalankan.

**Yang diperiksa di log:**

| Kelompok | Penanda yang dicari |
| --- | --- |
| NIK | NIK Siti, NIK Budi, NIK identitas sementara |
| Identitas sementara | Nama dan telepon peserta sementara |
| Nilai isian | `77.7`, `77,7`, kesimpulan pemeriksaan |
| Isi resume | Kondisi umum, *HBsAg*, rekomendasi, rekomendasi kerja |
| Tujuan penyerahan | Nomor WhatsApp (dengan dan tanpa tanda hubung), alamat email |
| Alasan tindakan (di luar `AT-MCU-45`) | Alasan jadwal ulang, batal, kembalikan, cabut, buka ulang, ganti dokter |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`roadmap/backend-roadmap.md` (`BE-MCU-035`), `02-backend-architecture.md` §11, `contracts/permission-audit-matrix.md`
(kunci `MCU-PERM-v1`), `roadmap/requirement-traceability.md` (`NFR-006`, `GAP-MCU-05`), `Services/Logging/LoggerService.cs`
(yang benar-benar tertulis: teks pesan + scope; payload hanya lewat kunci `Id`/`Name`/`Path`/…),
`Services/Security/PermissionRegistryValidator.cs`, `PermissionRegistryDescriptor.cs`, uji preseden
`MembershipPermissionRegistryTests`, seluruh controller MCU (32 endpoint ubah) dan kalimat log-nya (`McuHttp.RespondAsync`).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| — | Tidak ada berkas aplikasi yang berubah: tidak ditemukan kebocoran |
| `Tests/QuilvianSystemBackend.McuTests/` (sementara) | Penangkap log `PenangkapLog` + `McuPrivasiLogDanRegistriTests` (3 uji); dihapus sesudah lulus sesuai instruksi pemilik, salinan disimpan di scratchpad sesi |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` |
| Database | `NOT APPLICABLE` — tidak ada eksekusi ke basis data mana pun |
| Keamanan/Privasi | Terbukti: tidak ada data sensitif maupun alasan tindakan di log aplikasi pada alur MCU lengkap, termasuk log modul mitra |

### 3.4 Delta terhadap roadmap

| Butir | Roadmap | Implementasi | Alasan |
| --- | --- | --- | --- |
| Lokasi uji | *Uji di `Tests/`* | Uji dijalankan di `Tests/` lalu dihapus; salinan di scratchpad | Instruksi pemilik: folder uji MCU dihapus sesudah teruji |
| Kunci `MCU-PERM-v1` | *Seluruh kunci* | 28 kunci rilis MVP terdeklarasi; `McuOrder : Import`, `McuOrder : Recommend`, `McuMedicalFeeSource : Read` belum punya endpoint | Milik `EPIC MCU-07`, `09`, `10` — `POST-MVP` / menunggu `MCU-COORD-010` menurut roadmap |
| `GAP-MCU-05` | *Memeriksa ulang daftar tindakan ubah* | Ke-22 endpoint ubah data peserta dijalankan dan masing-masing meninggalkan kalimat log ber-aksi (15 kunci aksi). Kelengkapan **riwayat status** per tindakan tidak diuji ulang di sini | Riwayat sudah dibuktikan per task di laporannya masing-masing |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — tidak ada endpoint baru maupun berubah.

---

## 5. Verifikasi

Harness uji **sementara** dipulihkan dari salinan `BE-MCU-034` (SQLite, service produksi MCU/Lab/Rad/Billing/Registrasi),
ditambah penangkap log yang dipasang sebagai `ILoggerProvider` sehingga menerima log `LoggerService` **dan** `ILogger`
langsung milik modul mana pun, termasuk isi scope yang ikut terkirim ke Loki.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet test Tests/QuilvianSystemBackend.McuTests` | 51 lulus (3 baru + 48 regresi `024`, `030`–`034`) | `PASS` | `Passed! - Failed: 0, Passed: 51` |
| `AT-MCU-45` | 22 endpoint ubah lewat controller; 15 kunci aksi log MCU tertangkap; **nol** baris log memuat NIK (3), nama/telepon identitas sementara, nilai isian, kesimpulan, isi resume (4), tujuan WhatsApp/email | `PASS` | `AT_MCU_45_…` |
| Alasan tindakan | **Nol** baris log (MCU maupun mitra) memuat salah satu dari 9 alasan tindakan | `PASS` | `Alasan_tindakan_tidak_tertulis_…` |
| Registri izin | `PermissionRegistryValidator` atas rakitan aplikasi: nol temuan MCU (metadata, duplikat modul, `AccessType`, endpoint naked, tak ditegakkan); kunci `Mcu*` terdeklarasi **sama persis** dengan 28 kunci MVP; seluruh resource MCU satu modul | `PASS` | `Registri_izin_MCU_…` |
| Uji mutasi log | Kalimat log penyerahan diberi tujuan dan kalimat log approve diberi kondisi umum → `AT-MCU-45` **gagal** dan menunjukkan baris pesan serta scope-nya; dipulihkan identik (`cmp`) | `PASS` | Keluaran `dotnet test` |
| Uji mutasi registri | `[AccessAction]` `ApproveQuotaOverride` dilepas → uji registri **gagal** dengan temuan validator; dipulihkan identik | `PASS` | Keluaran `dotnet test` |
| Sink log sungguhan (Serilog/Loki), startup devYoga | Tidak dijalankan; penangkap membaca pesan dan scope yang sama yang diteruskan ke sink | `NOT RUN` | — |

Uji manual: `NOT APPLICABLE`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-MCU-45` lulus — log tanpa NIK, nilai isian, isi resume, tujuan penyerahan | Terpenuhi | §5 |
| Validator registri izin lulus | Terpenuhi untuk seluruh kunci MVP; 3 kunci `POST-MVP` belum punya endpoint (§3.4) | §5 |
| DoD: kriteria terbukti | Terpenuhi | §5 |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Uji ini **dihapus** bersama folder uji MCU sesuai instruksi; perlindungannya tidak berjalan otomatis pada perubahan berikutnya. Salinan di scratchpad sesi dapat dipulihkan untuk uji ulang |
| Masalah yang diketahui | Tiga kunci `MCU-PERM-v1` `POST-MVP` belum punya endpoint. Log penyerahan menyebut ID order (GUID), bukan nomor order — tidak sensitif, tetapi tidak seragam dengan log lain |
| Risiko tersisa | Endpoint MCU baru di masa depan tidak terlindungi uji ini kecuali uji dipasang permanen |
| Perubahan sampingan | Folder uji sementara dipulihkan lalu dihapus (salinan diperbarui di scratchpad); dua sisipan uji yang gagal kompilasi (string berbaris) diperbaiki sebelum dijalankan |
| Interupsi | Penghapusan folder uji sempat ditolak karena direktori kerja shell berada di dalamnya; diulang dari luar |
| Status Git | Tidak ada perubahan aplikasi dari task ini; laporan ini dan tanda roadmap/traceability. Perubahan `033`/`034` dan koreksi laporan `030`–`032` masih di working tree |
| Langkah berikutnya | Seluruh task backend `MVP-0`..`MVP-3` selesai. Sisa: keputusan kontrak batal penuh sesudah batal komponen (`BE-MCU-024`), `MCU-OQ-003` (bentuk bukti persetujuan), pemeriksaan hak akses MCU di devYoga, dan uji berjalan di devYoga |
