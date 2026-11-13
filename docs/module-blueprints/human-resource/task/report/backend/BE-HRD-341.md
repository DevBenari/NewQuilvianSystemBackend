# Laporan Perubahan Backend — `BE-HRD-341`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-HRD-341` |
| Judul | Pemberitahuan dampak penempatan sementara ke Kepala Unit asal |
| Slice | Amandemen `A52` atas `S-B4` Penjadwalan — `EPIC HRD-110`, gelombang `MVP-67` |
| Roadmap | `roadmap/backend-roadmap.md` bagian 24, Grafik B85 |
| Trace | `HRD-DEC-213`, `HRD-ASM-82`; `INT-HR-104`; `02-backend-architecture.md` 35.4.2 (`TemporaryPlacementRosterImpactService`, `WfpOrganizationAssignmentController`), 35.9.1; `AT-HRD-PK-45` |
| Contract version | `v26` — `approved` (pengguna, 12 November 2026) |
| Dependency | `BE-HRD-337` ✅ |
| Klasifikasi | `MEDIUM` — skor 5: satu repository (0), sekitar 10 berkas diperiksa (1), 3 berkas source (1), logika lintas sub-modul (1), efek samping endpoint yang ada (1), tanpa schema (0), tanpa keamanan baru (0), tanpa UI (0), ditambah 1 untuk integrasi pemberitahuan |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend`: `Areas/Corporate/HumanResource/SchedulingManagement/Services/`, `Areas/Corporate/HumanResource/WorkforceCore/Controllers/WfpOrganizationAssignmentController.cs`, `Program.cs`; dokumen roadmap/traceability/laporan |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `400a9678` (branch `yoga`) + perubahan `BE-HRD-339`, `BE-HRD-340`, migration `BE-HRD-FIX-001` yang belum di-commit |
| Tanggal | 13 November 2026 |
| Status | ✅ Selesai — harness 65/65 (termasuk kontrol negatif) dan `AT-HRD-PK-45` lulus lewat API devYoga |

### Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `Corporate` |
| Module / Submodule | `HumanResource` / `SchedulingManagement` (service baru), `WorkforceCore` (satu pemanggilan sesudah simpan) |
| Pemilik / prefix registry | `Human Resource` / `Hrd` (`ACTIVE / LEGACY`) |
| Keberlakuan | `NEW CODE` untuk `TemporaryPlacementRosterImpactService`; `TOUCHED LEGACY` untuk `WfpOrganizationAssignmentController` (konstruktor + dua pemanggilan) dan `Program.cs` |
| QBE yang berlaku | `QBE-SVC-001` (logika di service; controller hanya memanggil sesudah commit) |
| Sumber governance | `AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`; registry; `rules/backend/` |

---

## 1. Masalah yang diperbaiki

| Cacat sebelum task ini | Akibat |
| --- | --- |
| Penempatan sementara yang dicatat sesudah roster unit asal terbit tidak memberi tanda apa pun | Kepala Unit asal tidak tahu pegawainya tidak boleh lagi bertugas di unitnya; shift terbit itu tetap tercatat padahal pegawainya pindah sementara, dan baru ketahuan saat absen |

---

## 2. Proses bisnis

**Contoh (`HRD-DEC-213`, `HRD-ASM-82`).** Roster ICU Maret sudah terbit dan Tono terjadwal Pagi
15 Maret. Admin HRD lalu mencatat penempatan sementara Tono ke IGD 15–16 Maret.

1. Penempatan disimpan seperti biasa (commit).
2. Sistem mencari shift Tono yang aktif, tidak batal, bukan hari libur, pada 15–16 Maret, di roster
   **terbit** milik unit **selain** unit tujuan (IGD).
3. Untuk setiap roster yang terdampak, Kepala Unit pemiliknya — pemegang penempatan berjabatan di unit
   itu yang berlaku hari ini (35.9.1) dan punya akun aktif — menerima pemberitahuan: "Raka Firmansyah
   ditempatkan di Instalasi Gawat Darurat 15–16 Maret 2027, tetapi masih terjadwal 1 shift di roster …
   (ICU): 15 Mar. Lakukan Ganti Petugas; roster tidak diubah otomatis."
4. Roster tidak diubah. Bila Kepala Unit tidak melakukan apa-apa, pemeriksaan `B3` tetap menolak
   perubahan berikutnya yang memuat Tono pada tanggal itu.

Dijalankan sesudah penempatan `Temporary` aktif dibuat **atau** diubah. Pemberitahuan memakai kunci
dedup per penempatan, roster, dan rentang tanggal, sehingga menyimpan ulang tanpa mengubah rentang
tidak menggandakan pemberitahuan, sedangkan mengubah rentang memberi pemberitahuan baru. Unit yang
tidak punya Kepala Unit berakun dicatat di log sebagai peringatan. Kegagalan apa pun (basis data atau
pusat pemberitahuan) hanya dicatat di log dan tidak membatalkan penempatan.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `00-interview-decisions.md` `HRD-DEC-213`, `HRD-ASM-82`; `integration-contract.md` `INT-HR-104`; `02-backend-architecture.md` 35.4.2, 35.9.1; `acceptance-test-matrix.md` `AT-HRD-PK-45`
- `WfpOrganizationAssignmentController.cs` (Create, Update, transaksi, sinkronisasi proyeksi), `WfpOrganizationAssignment.cs`
- `HrdUnitScopeService.cs` (definisi Kepala Unit = penempatan berjabatan yang berlaku hari ini)
- `UnifiedNotificationGatewayService.cs`, `HrdNotificationService.cs` (penyimpanan `HrdNotification`, dedup)
- `RosterValidationService.FormatRange` (format rentang tanggal yang sama dengan pesan `B3`)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/HumanResource/SchedulingManagement/Services/TemporaryPlacementRosterImpactService.cs` | Baru. `NotifyOriginUnitsAsync(organizationAssignmentId)` sesuai bagian 2; mengembalikan ringkasan (`AffectedShiftCount`, `NotifiedUserIds`, `FailedNotificationCount`, `UnitsWithoutRecipient`, `Failed`); tidak pernah melempar galat; tidak menulis ke tabel mana pun selain lewat gateway pemberitahuan |
| `Areas/Corporate/HumanResource/WorkforceCore/Controllers/WfpOrganizationAssignmentController.cs` | Konstruktor menerima service baru; satu pemanggilan sesudah `CommitAsync` pada `Create` dan pada `Update` |
| `Program.cs` | `AddScoped<TemporaryPlacementRosterImpactService>()` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Sesuai `v26` (efek samping `organization-assignments`, **Diubah**). Request dan response tidak berubah |
| Database | Tanpa migration. Menulis `HrdNotification` lewat gateway; `TrxShiftAssignment` tidak disentuh |
| Keamanan/Auth | Tidak berubah. Isi pemberitahuan hanya nama, unit, rentang, dan tanggal shift |

---

## 4. Dokumentasi endpoint

Tidak ada endpoint baru. Efek samping pada endpoint yang ada:

### Corporate / Human Resource / Workforce Core / Organization Assignment

Base URL: `api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/organization-assignments`

| Method | Path | Hak akses | Efek samping baru |
| --- | --- | --- | --- |
| `POST` | `/` | `WfpOrganizationAssignment : Create` | Bila `AssignmentType = Temporary` dan aktif: pemberitahuan ke Kepala Unit asal yang roster terbitnya memuat pegawai pada rentang itu |
| `PUT` | `/{id}` | `WfpOrganizationAssignment : Update` | Sama, dihitung dari nilai sesudah diubah |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` | 0 galat, 603 peringatan (tetap) | `PASS` | Keluaran build |
| Harness xUnit SQLite di memori (65 uji: 59 sebelumnya + 6 baru) | 65/65 lulus | `PASS` | `dotnet test` di `C:\Users\Admin\AppData\Local\Temp\claude\d823ad68bb5a8f68\5d4a0fb6-9e9b-47eb-9814-7cfdf33f8519\scratchpad\scopetest` |
| — `AT-HRD-PK-45`: Tono `Temporary` HCU 1–14 Maret; hanya roster ICU terbit 5 Maret (bukan draf 6 Maret, bukan roster HCU, bukan 20 Maret) yang dihitung; satu pemberitahuan ke akun Sari berisi "Tono ditempatkan di HCU 1–14 Maret 2027", "5 Mar", "Ganti Petugas"; unit IGD tanpa Kepala Unit tercatat; sidik seluruh `TrxShiftAssignment` identik | Sesuai | `PASS` | `Penempatan_sementara_memberi_tahu…` |
| — Pusat pemberitahuan melempar galat → tidak melempar ke pemanggil, roster tetap | Sesuai | `PASS` | `Pemberitahuan_gagal…` |
| — Galat basis data (konteks sudah dibuang) → tidak melempar, `Failed = true` | Sesuai | `PASS` | `Galat_basis_data…` |
| — Penempatan `Secondary` atau `Temporary` nonaktif → tanpa pemberitahuan; penempatan ke unit roster sendiri → Kepala Unit itu tidak diberi tahu | Sesuai | `PASS` | Tiga uji |
| Kontrol negatif: syarat "hanya roster terbit" dicabut, backend dibangun ulang | Tepat 2 uji dampak yang gagal. Kode dipulihkan (dibandingkan byte demi byte), dibangun ulang, 65/65 lagi | `PASS` | Keluaran `dotnet test` |
| `AT-HRD-PK-45` lewat API devYoga (13 November 2026): `POST organization-assignments` Tono (Raka Firmansyah) `Temporary` IGD 15–16 Maret 2027; Tono terjadwal Pagi 15 Maret di roster terbit `ROST-202611-0002` | 200. Satu `HrdNotification` untuk akun Kepala Unit ICU uji (Cahyo Pamungkas) berisi teks pada bagian 2. Sidik md5 9 shift roster ICU identik sebelum dan sesudah | `PASS` | `at-log.jsonl` |
| — `PUT` dengan rentang 15–17 Maret, lalu `PUT` ulang tanpa perubahan rentang | Keduanya 200; pemberitahuan 1 → 2 → 2 (yang ketiga tertahan dedup); sidik roster tetap identik | `PASS` | `at-log.jsonl` |
| Log backend | Tiga baris "1 shift terbit terdampak, 1 pemberitahuan terkirim, 0 gagal"; tanpa galat selain `AttendanceScheduler:SystemActorUserId` yang sudah ada | `PASS` | `be.log` sesi |

Catatan: angka "terkirim" di log menghitung panggilan ke gateway; pemberitahuan yang sama dengan kunci
dedup yang sama tidak menambah baris di pusat pemberitahuan (terlihat pada `PUT` kedua).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Kegagalan pemberitahuan tidak membatalkan penyimpanan penempatan | Terpenuhi | Service tidak pernah melempar (harness: gateway gagal, basis data gagal); dipanggil sesudah commit |
| Tidak ada perubahan `TrxShiftAssignment` | Terpenuhi | Sidik identik di harness dan devYoga |
| `AT-HRD-PK-45` | Terpenuhi | Bagian 5 |
| DoD umum: build lulus, tanpa angka bisnis tertanam, tanpa kolom sensitif di log | Terpenuhi | Batas 10 tanggal dalam teks pemberitahuan adalah batas tampilan |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan baru |
| Masalah yang diketahui | (1) Pemanggilan diletakkan di dalam blok `try` controller yang sudah ada, sesudah commit. Service dijamin tidak melempar, tetapi pola controller itu sendiri bermasalah: galat apa pun sesudah commit (mis. sinkronisasi proyeksi, temuan `BE-HRD-339`) memicu `RollbackAsync` pada transaksi yang sudah selesai dan menjawab 500 walaupun data tersimpan. Temuan WorkforceCore, di luar cakupan. (2) Penempatan yang dibatalkan atau dipendekkan tidak mengirim pemberitahuan "pegawai kembali"; `HRD-ASM-82` hanya mewajibkan pemberitahuan dampak |
| Risiko tersisa | (1) Unit tanpa Kepala Unit berakun hanya tercatat di log; tidak ada penerima cadangan (mis. Admin HRD). (2) Kepala Unit ditentukan dari penempatan berjabatan yang berlaku hari ini, bukan pada tanggal shift |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | 1 berkas baru + 2 berkas diubah untuk task ini, di atas perubahan `BE-HRD-339`/`340`; laporan ini; 2 dokumen roadmap/traceability. Tidak ada stage atau commit |
| Langkah berikutnya | `BE-HRD-342` — setelan kehadiran sebagai satu-satunya sumber toleransi dan jendela |

Data uji devYoga baru (bertanda `UJI-A52`): penempatan berjabatan ICU untuk Cahyo Pamungkas
(`UJI-A52-KU-ICU-Cahyo`, sisip langsung ke basis data agar tidak memicu sinkronisasi proyeksi; kini
Cahyo memegang cakupan kelola ICU dan dapat dipakai untuk `AT-HRD-PK-09`/`-10`); penempatan
`Temporary` IGD 15–17 Maret 2027 untuk Raka Firmansyah (`UJI-A52-IGD-Tono-Temp2`); dua
`HrdNotification` untuk akun Cahyo.
