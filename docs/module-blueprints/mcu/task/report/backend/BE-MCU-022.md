# Laporan Perubahan Backend — `BE-MCU-022`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-MCU-022` |
| Judul | Komponen: daftar, sinkron, tambah, jadwal ulang |
| Slice | `MVP-2` — `EPIC MCU-04` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian *`BE-MCU-022`* |
| Trace | `FR-MCU-033`, `FR-MCU-037`, `FR-MCU-023` (komponen dijadwal ulang); `MCU-DEC-013`, `029`, `036` |
| Contract version | `MCU-API-v1` grup Mcu Examination (`GET/POST components`, `POST components/refresh`, `PATCH components/{componentId}/reschedule`); `MCU-STATE-v1` bagian 1.1 dan 2; `MCU-VAL-v1` `VAL-MCU-06`, `07`, `10`, `38`, `46`; `MCU-PERM-v1` (`McuExamination : Read/AddComponent/Update`); `MCU-INT-v1` `INT-MCU-05`..`07` — seluruhnya `approved` 2026-10-21 |
| Dependency | `BE-MCU-021` ✅ |
| Klasifikasi | `MEDIUM` — skor 9: berkas diperiksa 9–20 (1), berkas diubah 4–8 (1), logika sedang (1), empat endpoint baru (2), lintas modul Lab/Rad (2), transaksi + konkurensi (1), aturan identitas dokter (1) |
| Task mode | `BACKEND` |
| Target tulis | Backend: `McuExaminationService.cs`, `McuExaminationController.cs`, `McuExaminationDtos.cs`, `McuOrderQueryService.cs`, `McuOperationResult.cs`; laporan ini; tanda roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `e86d98aa` (branch `yoga`) — tidak bergeser |
| Tanggal | 2026-10-22 |
| Status | Selesai — `AT-MCU-29` dan `AT-MCU-07` terbukti di harness; penetapan harga Rp 75.000 oleh Billing tidak dijalankan (lihat §5) |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `MedicalCheckupManagement / Medical Check-Up`, prefix `Mcu`, registry `ACTIVE` |
| Keberlakuan | `NEW CODE` (Lab/Rad dipanggil lewat jalur publik dan jalur bertanda `BE-MCU-020`; identitas dokter lewat `InpatientClinicalContextService.ResolveActorDoctorIdAsync` yang sudah ada; tidak ada perubahan modul mitra) |
| QBE yang berlaku | `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001` (`McuExamination : Read/AddComponent/Update` + syarat dokter yang ditetapkan di service), `QBE-LOG-001`, `QBE-VAL-001`, `QBE-TXN-001`, `QBE-CODE-001`..`006`, `QBE-DTO-001` |
| Checker QBE | `PASS` — 5 berkas task nol temuan. Kontrol negatif sementara `ZzzKontrol` terkena `QBE-ENT-001`, `QBE-CFG-001`, `QBE-MOD-002`, lalu dihapus |

---

## 1. Masalah yang diperbaiki

Sesudah *Mulai pemeriksaan*, order tidak tahu kapan komponen Lab/Rad selesai, dokter MCU tidak dapat menambah
pemeriksaan, dan komponen yang perlu dipindah tanggal tidak punya jalan selain saat check-in.

**Contoh.** dr. Rina menambah Kolesterol Total untuk Budi → komponen *di luar paket*, pesanan Lab **tanpa**
tanda paket sehingga Lab menerbitkan fakta tagih `LaboratoryCharge` saat spesimen diterima; kelengkapan
*4 dari 5*. Gula Darah Puasa Budi dipindah ke Selasa yang kuotanya penuh (30/30) → berhasil, kuota Selasa
tetap 30.

---

## 2. Proses bisnis

1. **Daftar komponen** — status, cakupan (paket/di luar paket), tanggal, nomor pesanan Lab/Rad.
2. **Muat ulang status** (tanpa polling):
   - komponen susulan yang tanggalnya tiba → *Menunggu*, lalu dipesan (bertanda paket) bila pemeriksaan sudah
     dimulai;
   - komponen *Dipesan* → *Selesai* bila pesanan Lab `Completed` atau pesanan Rad `Completed` (laporan final);
   - semua komponen tuntas → order *Menunggu Resume*, riwayat oleh sistem (*"Semua komponen tuntas."*).
3. **Tambah pemeriksaan di luar paket** — hanya dokter MCU yang ditetapkan: Lab lewat pesanan Lab biasa, Rad
   lewat pesanan Rad biasa (keduanya ditagih satuan), Pemeriksaan MCU langsung *Siap diisi*. Order
   *Menunggu Resume* kembali *Dalam Pemeriksaan* dengan alasan tercatat.
4. **Jadwal ulang komponen** — komponen yang belum dipesan/diisi dipindah ke tanggal susulan yang buka,
   tanpa memakai kuota.

**Jalur tidak normal:**

| Keadaan | Akibat |
| --- | --- |
| Penambah bukan dokter yang ditetapkan (dokter lain, atau akun tanpa dokter) | `403` `VAL-MCU-38` *"Hanya dokter MCU yang ditetapkan untuk peserta ini yang dapat melakukan tindakan ini."* |
| Tambah sebelum pemeriksaan dimulai | `409` *"Pemeriksaan belum dimulai."* |
| Prosedur bukan Lab / Rad tanpa modalitas / jenis Pemeriksaan MCU tidak aktif | `400` `VAL-MCU-06` (pesan sama dengan komponen paket) |
| Pemeriksaan yang sama sudah ada di order | `409` `VAL-MCU-07` *"Darah Lengkap sudah ada di order ini."* |
| Lab/Rad menolak pesanan (mis. kunjungan sudah selesai) | `409` `MCU-PARTNER-REJECTED`; komponen tidak tersimpan, versi order tetap |
| Jadwal ulang ke tanggal tutup / hari ini atau lampau / tanggal sama | `400` `VAL-MCU-10` *"MCU tutup pada …"* / *"Pilih tanggal susulan sesudah hari ini untuk …"* / *"Tanggal baru sama dengan tanggal sekarang."* |
| Jadwal ulang komponen *Dipesan*, *Selesai*, *Dibatalkan*, atau Pemeriksaan MCU yang sudah diisi | `409` dengan alasannya |
| `version` berbeda / order diubah bersamaan | `409` `VAL-MCU-46` |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`roadmap/backend-roadmap.md` (`BE-MCU-022`), `contracts/api-contract.md`, `state-transition-matrix.md` (1.1, 2.1–2.3),
`validation-matrix.md`, `permission-audit-matrix.md` (definisi *dokter MCU yang ditetapkan*), `integration-contract.md`
(`INT-MCU-05`..`07`), `05-screen-brief.md` 5.6, `02-backend-architecture.md` (`McuOrderComponent`),
`00-interview-decisions.md` (`MCU-DEC-013`, `029`, `036`), `testing/acceptance-test-matrix.md`,
`InpatientClinicalContextService.ResolveActorDoctorIdAsync`, `RadReportService` (kapan pesanan Rad `Completed`),
`LabOrderStatus`, `McuPackageService` (pesan `VAL-MCU-06`), `McuOrderQueryService`, `McuDailySchedule`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `MedicalCheckupManagement/Services/McuExaminationService.cs` | `GetComponentsAsync`, `RefreshComponentsAsync`, `AddComponentAsync`, `RescheduleComponentAsync`; penolong `PlaceOrdersAsync` (Lab per disiplin, Rad per komponen, bertanda paket atau jalur biasa) dipakai bersama mulai pemeriksaan, sinkron, dan tambah; dependensi `InpatientClinicalContextService`, `IHttpContextAccessor` |
| `MedicalCheckupManagement/Controllers/McuExaminationController.cs` | `GET components`, `POST components/refresh` (`Read`), `POST components` (`AddComponent`), `PATCH components/{componentId}/reschedule` (`Update`) |
| `MedicalCheckupManagement/DTOs/McuExaminationDtos.cs` | `AddMcuComponentRequest`, `RescheduleMcuComponentRequest` |
| `MedicalCheckupManagement/Services/McuOrderQueryService.cs` | Pemuat komponen diekstrak menjadi `GetComponentsAsync` yang dipakai detail order dan endpoint komponen (perilaku detail tidak berubah) |
| `MedicalCheckupManagement/Services/McuOperationResult.cs` | Kode `VAL-MCU-38` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Empat endpoint baru sesuai kontrak; `McuOrderComponentResponse` tidak berubah |
| Database | Tanpa migration. **Tidak ada eksekusi ke basis data mana pun** |
| Keamanan/Auth | Tambah komponen = hak `McuExamination : AddComponent` **dan** akun tertaut ke dokter yang ditetapkan (`ApplicationUser.DoctorId`, klaim, profil SDM, atau surel — urutan pembaca yang sudah ada). Hak akses baru belum diberikan ke jabatan mana pun |

### 3.4 Delta terhadap kontrak

| Butir | Kontrak | Implementasi | Alasan |
| --- | --- | --- | --- |
| Dokter pesanan di luar paket | Tidak disebut | Pesanan Lab/Rad di luar paket **tanpa** `InstructingDoctorId` (jalur biasa) | Jalur yang mengisi dokter MCU hanya untuk pesanan bertanda (`MCU-COORD-008`); mengisinya untuk pesanan biasa butuh perubahan Lab/Rad yang belum disetujui. Akibat: nilai kritis Kolesterol tambahan tidak otomatis menyebut dr. Rina |
| Pemeriksaan ganda | `VAL-MCU-07` hanya untuk paket | Juga ditolak per order (kode `VAL-MCU-07`) | Mencegah pesanan dan tagihan ganda |
| Isi `RescheduleMcuComponentRequest` | Tidak dirinci | `scheduledDate` + `version` | `VAL-MCU-46` berlaku untuk semua tindakan ubah |
| Komponen yang boleh dijadwal ulang | *Komponen susulan* | *Menunggu*, *Dijadwal ulang*, atau *Siap diisi* yang belum diisi; order *Hadir* atau *Dalam Pemeriksaan* | Komponen yang sudah dipesan dibatalkan lewat batal komponen (`BE-MCU-024`), bukan dipindah |
| Pemesanan komponen susulan | *Sistem* | Terjadi saat **Muat ulang status** sesudah tanggalnya tiba | Kontrak tidak menyediakan pemicu lain dan tanpa polling (`05-screen-brief.md` 5.6) |
| Sinkron skrining | *"Sinkron status dari Lab/Rad dan skrining"* | Hanya Lab/Rad | Skrining menjadi syarat isi Pemeriksaan MCU dan approve — dibaca di `BE-MCU-023`/`030` |

---

## 4. Dokumentasi endpoint

#### Health Services / Medical Checkup Management / Mcu Examination (`.../orders/{id}`)

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/components` | Komponen + status + tautan pesanan | `McuExamination : Read` | - | `List<McuOrderComponentResponse>` | Tersedia |
| `POST` | `/components/refresh` | Sinkron status dari Lab/Rad, pesan komponen susulan yang tiba, order → *Menunggu Resume* bila tuntas | `McuExamination : Read` | - | `List<McuOrderComponentResponse>` | Tersedia |
| `POST` | `/components` | Tambah pemeriksaan di luar paket (dokter MCU yang ditetapkan) | `McuExamination : AddComponent` | `AddMcuComponentRequest` (componentType, procedureId?, examinationTypeId?, radModalityId?) | `McuOrderComponentResponse` | Tersedia |
| `PATCH` | `/components/{componentId}/reschedule` | Pindah tanggal komponen, tanpa kuota | `McuExamination : Update` | `RescheduleMcuComponentRequest` (scheduledDate, version) | `McuOrderComponentResponse` | Tersedia |

Kode status: `200`, `400`, `403`, `404`, `409`. Log aplikasi untuk tambah dan jadwal ulang: nama komponen, nomor
pesanan atau tanggal baru — tanpa NIK.

---

## 5. Verifikasi

Harness uji **sementara** dibuat ulang (`Tests/QuilvianSystemBackend.McuTests`: SQLite; `LabOrderService`,
`RadOrderService`, `LabSpecimenService`, `ClinicalMilestoneFactProducer`, `InpatientClinicalContextService`
produksi; akun dr. Rina/dr. Hadi tertaut lewat `ApplicationUser.DoctorId`; order dimulai lewat `StartAsync`
sungguhan), dijalankan, lalu **dihapus** sesuai instruksi user.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 error | `PASS` | Keluaran build |
| `dotnet test Tests/QuilvianSystemBackend.McuTests` | 12 lulus | `PASS` | `Passed! - Failed: 0, Passed: 12` |
| `AT-MCU-29` | Kolesterol Total → *di luar paket*, *Dipesan*, `AddedByDoctorId` = dr. Rina, nilai pengembalian 0; pesanan Lab **tanpa** `CoveringMcuOrderId` (pesanan paket tetap 1 bertanda); 4 komponen paket tuntas → **5 komponen, *4 dari 5***, order tetap *Dalam Pemeriksaan*; spesimen Kolesterol layak → tepat 1 fakta `LaboratoryCharge` untuk pesanan Kolesterol | `PASS` | `AT_MCU_29_…` |
| *Tagihan bertambah Rp 75.000* | Fakta tagih terbit dan diserahkan ke Billing; penetapan harga dan baris tagihan oleh Billing tidak dirakit di harness (jalur Billing yang sudah ada, tidak berubah) | `PARTIAL` | — |
| Tambah Rad dan Pemeriksaan MCU | USG Abdomen → pesanan Rad tanpa tanda; Audiometri → *Siap diisi*; order *Menunggu Resume* → *Dalam Pemeriksaan* dengan alasan *"Tambah pemeriksaan di luar paket: USG Abdomen."* | `PASS` | `Tambah_rad_dan_pemeriksaan_mcu_…` |
| `VAL-MCU-38` dan status | dr. Hadi dan akun petugas → `403` pesan persis; order *Hadir* → *"Pemeriksaan belum dimulai."*; nol pesanan/komponen tambahan | `PASS` | `Tambah_hanya_oleh_dokter_…` |
| Isian dan ganda | Prosedur non-Lab → `VAL-MCU-06`; Rad tanpa modalitas → pesan Rad; Darah Lengkap lagi → `409` *"Darah Lengkap sudah ada di order ini."* | `PASS` | `Tambah_menolak_isian_salah_…` |
| Lab menolak di tengah | Kunjungan *Completed* → `409` `MCU-PARTNER-REJECTED`; tetap 4 komponen, 1 pesanan Lab, versi order tetap | `PASS` | `Lab_menolak_tambahan_…` |
| Sinkron | Tanpa perubahan di Lab/Rad → tetap *Dipesan*; pesanan Lab/Rad `Completed` → komponen *Selesai* + `CompletedAt`; Pemeriksaan Fisik tuntas → order *Menunggu Resume*, riwayat tanpa pelaku dengan alasan *"Semua komponen tuntas."* | `PASS` | `Sinkron_menyelesaikan_…` |
| Komponen susulan tiba | *Dijadwal ulang* bertanggal hari ini → dipesan bertanda paket atas nama dr. Rina | `PASS` | `Sinkron_memesan_komponen_susulan_…` |
| Daftar / tidak ada | 4 komponen paket; order tak dikenal → `404` untuk daftar dan sinkron | `PASS` | `Daftar_komponen_…` |
| `AT-MCU-07` | GDP ke Selasa berkuota 30/30 → *Dijadwal ulang* bertanggal Selasa; `UsedQuota` tetap 30; versi order +1 | `PASS` | `AT_MCU_07_…` |
| Penolakan jadwal ulang | Tutup → `VAL-MCU-10`; hari ini → pesan tanggal susulan; tanggal sama; versi lama → `VAL-MCU-46`; komponen *Dipesan* → `409`; komponen tak dikenal → `404`; versi order tidak berubah | `PASS` | `Jadwal_ulang_menolak_…` |
| Regresi `BE-MCU-021` | Tunai tanpa tagihan → `VAL-MCU-34`; Rad menolak saat mulai → nol pesanan Lab, order *Hadir*, komponen tetap *Menunggu* | `PASS` | `Regresi_mulai_…` |
| Endpoint | Empat route dan pasangan izin sesuai kontrak; `PermissionRegistryValidator` nol temuan MCU | `PASS` | `Endpoint_komponen_…` |
| Uji mutasi | Penjaga `VAL-MCU-38` dilepas dan komponen tambahan dipesan bertanda → 3 uji **gagal**; dipulihkan identik (`cmp`), kembali 12/12 | `PASS` | Keluaran `dotnet test` |
| Checker QBE `-Path` (5 berkas + kontrol) | Nol temuan; kontrol terkena | `PASS` | Keluaran checker |
| Billing sungguhan, PostgreSQL, `403` lewat HTTP, Swagger | Tidak dijalankan | `NOT RUN` | Startup ke devYoga menunggu migration MCU |

Satu cacat ditemukan dan diperbaiki saat uji: sinkron tidak memindahkan order ke *Menunggu Resume* bila komponen
terakhir tuntas di luar sinkron (Pemeriksaan MCU diisi) dan tidak ada komponen Lab/Rad yang berubah. Syarat
keluar-awal kini ikut memeriksa *semua tuntas*.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `AT-MCU-29` — komponen di luar paket; pesanan Lab tanpa tanda; tagihan bertambah saat spesimen diterima; *4 dari 5* | Terpenuhi pada sisi MCU/Lab (fakta tagih terbit); nominal Rp 75.000 oleh Billing tidak diuji | §5 |
| `AT-MCU-07` — jadwal ulang ke Selasa penuh berhasil; `UsedQuota` tidak berubah | Terpenuhi | §5 |
| DoD: kriteria terbukti | Terpenuhi, dengan catatan nominal Billing | — |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Muat ulang status** memakai hak `Read` tetapi dapat membuat pesanan Lab/Rad untuk komponen susulan yang tanggalnya tiba (sesuai kontrak: endpoint ini `Read`, perpindahannya oleh *Sistem*) |
| Masalah yang diketahui | Pesanan Lab/Rad di luar paket tanpa dokter pemberi instruksi (§3.4); pesanan Lab/Rad yang dibatalkan dari modulnya tidak disinkronkan ke komponen (pembatalan komponen milik `BE-MCU-024`) |
| Risiko tersisa | Penetapan harga Billing untuk pemeriksaan tambahan belum diuji ujung ke ujung; konkurensi hanya terbukti di SQLite; hak akses `McuExamination` belum diberikan ke jabatan mana pun; startup backend ke devYoga masih menunggu migration MCU |
| Perubahan sampingan | Folder uji sementara dibuat dan dihapus; kontrol negatif QBE dibuat dan dihapus |
| Interupsi | Tidak ada |
| Status Git | Perubahan task di folder modul MCU (`??`, folder baru sejak `BE-MCU-002`); laporan ini; tanda roadmap/traceability |
| Langkah berikutnya | `BE-MCU-023` (isi Pemeriksaan MCU) kini siap |
