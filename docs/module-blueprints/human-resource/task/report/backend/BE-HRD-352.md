# Laporan Perubahan Backend — `BE-HRD-352`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-352` |
| Judul | Cadangan siaga dan catatan panggilan |
| Slice | Amandemen `A52` — `POST-MVP` / `EPIC HRD-115` (dijadwalkan pengguna: "lanjut task berikutnya", 16 November 2026) |
| Roadmap | `roadmap/backend-roadmap.md` bagian 24, Grafik B88 |
| Trace | `FR-PK-019`–`022`; `HRD-DEC-198`, `204`, `205`; `HRD-ASM-75`, `78`; `INV-A52-04`; kontrak 35.9.5 (`api-contract.md` On-Call `backups`, `calls`, `call-in`); state 30.2; `VR-PK-08`–`12`; `integration-contract.md` `INT-HR-39`, `INT-HR-40`; `testing/acceptance-test-matrix.md` `AT-HRD-PK-15`–`18` |
| Contract version | `v26` — `approved` (pengguna, 12 November 2026) |
| Dependency | `BE-HRD-335` ✅ (entity `HrdOnCallBackup`, `HrdOnCallCall`, `MstOnCallType.AnswerTimeoutMinutes`, nilai status) |
| Klasifikasi | `STANDARD` — skor 7: satu repository (0), sekitar 20 berkas diperiksa (2), 3 berkas baru + 4 berkas diubah (2), kontrak integrasi Lab (`INT-HR-39`) dan serah terima Payroll (`INT-HR-40`) disentuh (3), tanpa migration (0) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `SchedulingManagement` (DTO, service, controller), `Program.cs`, proyek uji HR; dokumen roadmap/traceability/laporan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `13b5d76b` (branch `yoga`) + perubahan `BE-HRD-344` s.d. `351` yang belum di-commit |
| Tanggal | 16 November 2026 |
| Status | ✅ Selesai — proyek uji HR 177/177 lulus (termasuk 9 uji siaga baru dan kontrol negatif); `AT-HRD-PK-15`–`18` lulus lewat API devYoga; 403 non-superadmin terbukti |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module / Submodule | `HumanResource` / `SchedulingManagement` |
| Pemilik / prefix registry | `Human Resource` / `Hrd` (`ACTIVE`; `Trx*` siaga `LEGACY`); entity `HrdOnCallBackup`, `HrdOnCallCall` sudah ada sejak `BE-HRD-334`/`335` — tidak ada entity atau tabel baru |
| Keberlakuan | `NEW CODE`: `OnCallCallService`, `OnCallCallDtos`; `TOUCHED LEGACY`: `OnCallAssignmentService`, `OnCallAssignmentController` |
| QBE yang berlaku | `QBE-SVC-001` (controller → service, tanpa `ApplicationDbContext` di controller), `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001` |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |

---

## 1. Masalah yang diselesaikan

| Keadaan sebelum task ini | Akibat |
| --- | --- |
| Penugasan siaga hanya punya satu set kolom ringkasan (`IsCalledIn`, `CallInTime`, `ResponseMinutes`) yang ditimpa setiap `call-in` | Panggilan kedua menghapus jejak panggilan pertama; penolakan dan panggilan yang tidak terjawab tidak tercatat sama sekali |
| Tidak ada cadangan siaga | Saat dokter siaga utama tidak menjawab, petugas IGD tidak tahu siapa yang dihubungi berikutnya |
| Tabel `HrdOnCallBackup`/`HrdOnCallCall` sudah ada (`BE-HRD-335`) tanpa endpoint | Keputusan `HRD-DEC-198`, `204`, `205` belum bisa dipakai |

---

## 2. Yang dibangun

| Berkas | Isi |
| --- | --- |
| `SchedulingManagement/DTOs/OnCallCallDtos.cs` (baru) | Permintaan cadangan (ganti utuh), panggilan, jawab terima/tolak, kedaluwarsa, tiba; respons panggilan dengan `NextBackup` (nama dan nomor telepon cadangan berikutnya) dan `NoBackupMessage` |
| `SchedulingManagement/Services/OnCallCallService.cs` (baru) | Catat panggilan (`Sent`), terima (`Accepted`), tolak (`Declined`), kedaluwarsa (`Expired`), tiba (`Completed`); `RowVersion` per catatan; batas jawab dari `MstOnCallType.AnswerTimeoutMinutes`; ringkasan penugasan diisi dari panggilan Diterima pertama (`HRD-ASM-75`); status penugasan **tidak pernah** diubah |
| `SchedulingManagement/Services/OnCallAssignmentService.cs` | `GetBackupsAsync`, `SetBackupsAsync` (ganti utuh dalam transaksi, `VR-PK-08`, penugasan `Completed`/`Cancelled` → 409); `RecordCallInAsync` kini juga menulis satu catatan `Completed` dan hanya mengisi ringkasan bila belum ada panggilan Diterima/Selesai yang lebih awal |
| `SchedulingManagement/Controllers/OnCallAssignmentController.cs` | 8 endpoint baru (bagian 4) dengan log audit tanpa alasan panggilan/penolakan |
| `Program.cs` | `AddScoped<OnCallCallService>()` |
| `Tests/QuilvianSystemBackend.HumanResourceTests/OnCallCallTests.cs` (baru) | 9 uji: `AT-HRD-PK-15`–`18`, `VR-PK-08`–`12`, 409, `call-in` lama, `INT-HR-39` |
| `Tests/.../A52PermissionRegistryTests.cs` | `OnCallAssignmentController` ikut validator registri, 2 aksi kontrak baru, uji 403 untuk 17 endpoint siaga |
| `Tests/.../AcceptanceTestIndex.md` | Pemetaan `AT-HRD-PK-15`–`18` |

Aturan dari kontrak:

| Aturan | Hasil | Pesan |
| --- | --- | --- |
| `VR-PK-08` | `422` | "Urutan cadangan dan nama cadangan tidak boleh ganda atau sama dengan siaga utama" |
| `VR-PK-09` | `422` | "Pegawai ini tidak sedang siaga pada penugasan tersebut" (penugasan tidak `Active`, atau yang dipanggil bukan siaga utama/cadangan) |
| `VR-PK-10` | `422` | "Batas jawab {n} menit belum lewat sejak panggilan HH.mm" (jam WIB) |
| `VR-PK-11` | `400` | "Alasan penolakan wajib diisi" |
| `VR-PK-12` | `400` | "Jam tiba tidak boleh lebih awal dari jam panggil" |
| Terima sesudah batas jawab | `422` | Arahkan ke kedaluwarsa lalu panggil cadangan (`HRD-DEC-198`) |
| Transisi dari status yang salah / `RowVersion` lama | `409` | — |
| Cadangan habis | `200` | `NoBackupMessage` = "Hubungi Kepala Unit pemilik jadwal siaga" |

---

## 3. Dampak

| Aspek | Dampak |
| --- | --- |
| `INT-HR-39` (dokter siaga aktif untuk Lab) | Tidak berubah — status penugasan tetap `Active`, cadangan tidak masuk daftar. Diuji di harness dan lewat API |
| `INT-HR-40` (ringkasan siaga ke Payroll) | Bentuk tidak berubah; isi kini dari panggilan Diterima pertama. `OnCallCompensationHandoffService` tidak disentuh |
| `POST /{id}/call-in` | Tetap ada, izin dan bentuk respons tetap; kini juga menulis catatan `Completed` (kontrak: **Diubah**) |
| Database | Tanpa migration; memakai tabel `BE-HRD-335` |
| Hak akses | Aksi baru `OnCallAssignment : ManageBackup` dan `RecordCall` muncul di registri; belum diberikan ke jabatan mana pun |

---

## 4. Endpoint

Prefix `api/v1/corporate/human-resource/scheduling-management/on-call-assignments` (alias lama tetap).

| Method | Path | Izin | Keterangan |
| --- | --- | --- | --- |
| `GET` | `/{id}/backups` | `OnCallAssignment : Read` | Daftar cadangan berurutan; tanpa nomor telepon |
| `PUT` | `/{id}/backups` | `OnCallAssignment : ManageBackup` | Ganti utuh daftar cadangan |
| `GET` | `/{id}/calls` | `OnCallAssignment : Read` | Catatan panggilan urut jam panggil |
| `POST` | `/{id}/calls` | `OnCallAssignment : RecordCall` | Catat panggilan (`Sent`) |
| `POST` | `/{id}/calls/{callId}/accept` | `OnCallAssignment : RecordCall` | `Sent` → `Accepted` |
| `POST` | `/{id}/calls/{callId}/decline` | `OnCallAssignment : RecordCall` | `Sent` → `Declined`; respons memuat cadangan berikutnya |
| `POST` | `/{id}/calls/{callId}/expire` | `OnCallAssignment : RecordCall` | `Sent` → `Expired` sesudah batas jawab; respons memuat cadangan berikutnya |
| `POST` | `/{id}/calls/{callId}/complete` | `OnCallAssignment : RecordCall` | `Accepted` → `Completed` dengan jam tiba |
| `POST` | `/{id}/call-in` | `OnCallAssignment : Update` | **Diubah**: juga menulis catatan `Completed` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build Tests/QuilvianSystemBackend.HumanResourceTests -p:RunAnalyzers=False` | 0 galat; tanpa peringatan baru dari kode task ini (CS1570 di `OnCallAssignmentService.cs:389` berasal dari `<summary>` lama berisi `&`) | `PASS` | Keluaran build |
| `dotnet test` proyek uji HR | **177/177 lulus** (5 m 56 d) | `PASS` | Keluaran `dotnet test` |
| — `OnCallCallTests` | 9/9: tiga transisi + status `Active` + `INT-HR-39` (`AT-15`); kedaluwarsa 5 menit `422`, 10 menit berhasil dengan cadangan 1 (`AT-16`); semua menolak → pesan Kepala Unit (`AT-17`); ringkasan dari Diterima pertama, respons 25 menit, Diterima kedua tidak menimpa (`AT-18`); `VR-PK-08`/`09`/`11`/`12`; `RowVersion` lama `409`; `call-in` lama | `PASS` | — |
| — Registri dan 403 | `ManageBackup`/`RecordCall` terdaftar; validator bersih untuk `OnCallAssignmentController`; 17 endpoint siaga menolak pegawai tanpa kebijakan | `PASS` | `A52PermissionRegistryTests` |
| — Regresi `INT-HR-39` | Lulus | `PASS` | `IntHr39RegressionTests` |
| Kontrol negatif gelombang 1: cek `VR-PK-10`, cek `RowVersion`, dan penjaga "Diterima pertama" dimatikan | Tepat 3 uji yang dituju gagal (kedaluwarsa, konkurensi, ringkasan); 5 lainnya lulus; kode dipulihkan identik (`cmp`) | `PASS` | Keluaran `dotnet test` |
| Kontrol negatif gelombang 2: ringkasan ikut mengubah status penugasan; `call-in` tidak menulis catatan | Uji `AT-15`, `call-in`, dan ringkasan gagal; dipulihkan identik | `PASS` | Keluaran `dotnet test` |
| Kontrol negatif 3: penjaga ringkasan pada `call-in` dimatikan | Uji `Call_in_sesudah_panggilan_diterima_tidak_menimpa_ringkasan` gagal; dipulihkan identik | `PASS` | Keluaran `dotnet test` |
| Startup Development | "Permission registry valid. 2439 identitas kanonik dari 2439 kemampuan terdaftar." | `PASS` | `be352.log` |
| API devYoga (16 November 2026), superadmin; penugasan uji `UJI-A52 BE-HRD-352` (`9a42ebc8…`) untuk Arif Maulana ("dr. Andi"), cadangan Bagas Saputra (1) dan Raka Firmansyah (2), jenis siaga `UJI-JAGA` (batas jawab 10 menit) | 39/40 cek lulus; satu cek gagal karena kueri pembantu salah nama tabel izin, diulang dengan nama benar → lulus | `PASS` | `at-352.mjs`, `at-log.jsonl` di `C:\Users\Admin\AppData\Local\Temp\claude\d823ad68bb5a8f68\5d4a0fb6-9e9b-47eb-9814-7cfdf33f8519\scratchpad\mig` |
| — `AT-HRD-PK-18` | Arif menolak ("Sedang operasi") → respons memuat Bagas urutan 1 dengan nomor telepon; Bagas menerima dan tiba 25 menit → ringkasan dipanggil = ya, respons 25, SLA terpenuhi, status `Active` | `PASS` | `at-log.jsonl` |
| — `AT-HRD-PK-16` / `VR-PK-10` | Kedaluwarsa 5 menit sesudah panggilan → `422` "Batas jawab 10 menit belum lewat sejak panggilan 15.02"; 11 menit → `Expired` | `PASS` | `at-log.jsonl` |
| — `AT-HRD-PK-17` | Raka menolak sesudah dua cadangan terpanggil → `nextBackup` kosong, "Hubungi Kepala Unit pemilik jadwal siaga" | `PASS` | `at-log.jsonl` |
| — `AT-HRD-PK-15` | Dikirim → Diterima → Selesai 20 menit; ringkasan tetap 25 (Diterima pertama); status `Active` | `PASS` | `at-log.jsonl` |
| — `INT-HR-39` | Daftar dokter siaga aktif identik sebelum/sesudah panggilan; Bagas (cadangan yang menerima) tidak masuk | `PASS` | `at-log.jsonl` |
| — Validasi | `VR-PK-08` tiga varian `422`; urutan 0 `400`; `VR-PK-09` `422`; alasan panggilan kosong `400`; `VR-PK-11` `400`; `VR-PK-12` `400`; tiba sebelum Diterima `409`; terima lewat batas `422`; `RowVersion` lama `409` | `PASS` | `at-log.jsonl` |
| — 401 / 403 | Tanpa sesi `401`; `gilang.mahendra` (32 butir izin asal, tanpa `OnCallAssignment`; sandi dari pengguna, tidak disimpan) → `403` untuk `PUT backups`, `POST calls`, `accept`, `GET calls`, `GET backups` | `PASS` | `at-log.jsonl` |
| — Log audit | 13 entri `[AUD]` untuk 13 aksi berhasil; nol kemunculan alasan panggilan maupun alasan penolakan | `PASS` | `be352.log` |
| — Isi DB | Catatan per status: `Completed` 3, `Declined` 2, `Expired` 1; cadangan aktif 1 = Bagas, 2 = Raka | `PASS` | `at-log.jsonl` |
| Temuan saat uji API: `call-in` sesudah panggilan Diterima menimpa ringkasan (25 → 7 menit) | Diperbaiki (bagian 2, `RecordCallInAsync`); uji harness baru + kontrol negatif 3 | Diperbaiki | — |
| API devYoga susulan, penugasan uji `bc16737b…` | Panggilan Diterima → Selesai 25 menit, lalu `call-in` 7 menit → respons `call-in` melaporkan 7 menit, ringkasan tetap 25, dua catatan `Completed` | `PASS` | `at-352b.mjs`, `at-log.jsonl` |

---

## 6. Acceptance criteria

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Panggilan dicatat per baris dengan status `Sent`/`Accepted`/`Declined`/`Expired`/`Completed` | Terpenuhi | Bagian 5 (isi DB) |
| Cadangan berurutan | Terpenuhi | `VR-PK-08`, `NextBackup` |
| Status penugasan tetap | Terpenuhi | `AT-15`, `AT-18`, kontrol negatif gelombang 2 |
| `INT-HR-39` tidak berubah | Terpenuhi | Harness + API |
| Ringkasan dari panggilan Diterima pertama | Terpenuhi — termasuk lewat `call-in` lama | `AT-18`, uji `call-in` susulan |
| `AT-HRD-PK-15`–`18` | Terpenuhi | Harness + API |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada dari kode task ini |
| Masalah yang diketahui | (1) Aksi `OnCallAssignment : ManageBackup/RecordCall` belum diberikan ke jabatan mana pun di devYoga — petugas IGD/Kepala Unit belum bisa memakainya sebelum Akses Role diatur. (2) Penugasan uji pertama (`9a42ebc8…`) menyimpan ringkasan 7 menit dari `call-in` sebelum perbaikan — data uji, sudah `Completed` |
| Risiko tersisa | Kedaluwarsa dicatat manual oleh petugas (tidak ada penjadwal otomatis) — sesuai kontrak; jam panggil tidak dibatasi rentang penugasan karena `VR-PK-09` hanya mensyaratkan penugasan `Active` |
| Perubahan sampingan | `RecordCallInAsync` hanya mengisi ringkasan bila belum ada panggilan Diterima/Selesai yang lebih awal (`HRD-ASM-75`); tanpa perubahan bentuk respons |
| Interupsi | Pemeriksa izin otomatis shell beberapa kali tidak memberi putusan; dilanjutkan dari kondisi build yang terverifikasi |
| Status Git | Baru: `OnCallCallService.cs`, `OnCallCallDtos.cs`, `OnCallCallTests.cs`, laporan ini. Diubah: `OnCallAssignmentService.cs`, `OnCallAssignmentController.cs`, `Program.cs`, `A52PermissionRegistryTests.cs`, `AcceptanceTestIndex.md`, roadmap, traceability. Tidak ada commit |
| Langkah berikutnya | `BE-HRD-357` (bergantung `BE-HRD-336` ✅) bila dijadwalkan; `BE-HRD-353`–`356` terblokir `BE-HRD-343`; frontend `FE-HRD-317` |

Data uji devYoga baru (bertanda `UJI-A52 BE-HRD-352`): dua penugasan siaga Arif Maulana di roster `ROST-202611-0005`
(`9a42ebc8-6a75-43d4-959f-2010401abeea`, `bc16737b-f8ff-4fc5-9e08-15f72a585a6d`), keduanya sudah `Completed`
sehingga tidak muncul di daftar dokter siaga aktif; 2 cadangan dan 8 catatan panggilan pada keduanya. Hak akses
tidak diubah.
