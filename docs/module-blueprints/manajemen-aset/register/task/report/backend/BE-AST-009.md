# Laporan Perubahan Backend — `BE-AST-009`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-AST-009` |
| Judul | Penyerahan dan penempatan |
| Slice | `REG-S2` — Register aset: pencatatan, label, penempatan, kondisi, dokumen, pemanfaatan, impor, dashboard |
| Roadmap | `register/roadmap/backend-roadmap.md` — kartu `BE-AST-009` |
| Trace | `CAP-04.1`–`04.4`; `AST-DEC-044`, `AST-DEC-046`, `AST-DES-006`; API 2.9, 2.10; `state-transition-matrix` §4; `validation-matrix` §3 (`AST-VAL-REG-020..026`); `permission-audit-matrix` (`AssetPlacement`); `AT-REG-04`, `05`, `36..38` |
| Contract version | `AST-REG-1.0.0` `approved` 2026-11-12 |
| Dependency | `BE-AST-007` ✅, `BE-AST-005` ✅ (kunci proses, linimasa, penomoran) |
| Klasifikasi | `HEAVY` — 1 aggregate ber-lifecycle (4 transisi), 2 controller (11 endpoint), PDF berita acara, titik panggil lintas sub-modul; tanpa entity atau migration baru |
| Task mode | `BACKEND`, diminta Rizki 2026-11-17 ("lanjutkan pengerjaan kita yang tadi") |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/AssetManagement/**`; laporan ini dan baris status roadmap/traceability |
| Model | Claude Opus 5.5 (`claude-opus-5-5`) |
| Commit backend saat dikerjakan | `0c84bacf` (branch `rizkiG`, sesudah merge `QuilvianIntegrationBackend`) |
| Tanggal | 17 November 2026 |
| Status | ✅ SELESAI 17 November 2026 — keenam kriteria terpetakan ke source; QBE Strict `PASS`; build Rizki berhasil; uji runtime 33/33 |

**Backend Governance Preflight**

| Butir | Nilai |
| --- | --- |
| Area / Module / Submodule | `Corporate` / `AssetManagement` / `Register` |
| Pemilik / prefix registry | `AssetManagement / Register Aset`, prefix `Ast`, `ACTIVE` |
| Keberlakuan | `NEW CODE` |
| QBE yang berlaku | `QBE-MOD-001`, `QBE-SVC-001`, `QBE-API-001`, `QBE-PERM-001`, `QBE-LOG-001`, `QBE-CODE-001`/`002`/`003` (nomor `AST-PNY` lewat deret bersama), `QBE-VAL-001`, `QBE-TXN-001`, `QBE-DTO-001`, `QBE-DEL-001` (batal, bukan hapus) |
| Tidak berlaku | `QBE-ENT-*`, `QBE-CFG-*`, `QBE-NAM-*`, `QBE-DB-*` — model `AstAssetHandover`, `AstAssetHandoverItem`, `AstAssetPlacement` sudah ada sejak `BE-AST-003`/`004`; tidak ada migration |

---

## 1. Masalah yang diperbaiki

Sesudah `BE-AST-007`, aset yang disetujui berstatus **Belum Ditempatkan** dan tidak punya unit, ruangan, maupun
penanggung jawab. Belum ada cara memindahkannya ke tangan unit. Akibatnya:

- Kepala Unit tidak melihat aset unitnya (daftar inventaris tersaring per unit kosong untuk unit mana pun).
- `AT-REG-12`/`46` (batas unit dengan aset yang sudah ditempatkan) tidak dapat diuji.
- Sub-modul pergerakan (mutasi) belum punya jalur bersama untuk mengubah penempatan.

---

## 2. Proses bisnis

**Pelaku:** Petugas Aset / Kepala Bagian Aset (menyerahkan, membatalkan), Kepala Unit tujuan atau pegawai penanggung
jawab yang dituju (mengonfirmasi, menolak terima).

| Langkah | Pelaku | Yang terjadi |
|---|---|---|
| 1 | Petugas Aset | Memilih 1–200 aset `Terdaftar`, tujuan (**Penempatan Awal** atau **Pindah Dalam Unit**), unit, ruangan milik unit itu, dan pegawai penanggung jawab |
| 2 | Sistem | Memeriksa setiap aset (bagian "Jalur tidak normal"), mengunci aset dengan proses **penyerahan**, menerbitkan nomor `AST-PNY-######`, menyimpan salinan kondisi tiap aset. Status **Diserahkan** |
| 3 | Penerima yang dituju | Melihat penyerahan bertanda "menunggu konfirmasi Anda" (`awaitingMyConfirmation`) |
| 4a | Penerima | **Konfirmasi** → dalam satu transaksi untuk setiap aset: penempatan berjalan lama ditutup, penempatan baru dibuka, kolom "saat ini" aset diperbarui, `Ditempatkan`, kunci dilepas, linimasa ditulis. Status **Dikonfirmasi** |
| 4b | Penerima | **Tolak terima** beralasan → kunci dilepas, penempatan tidak berubah. Status **Ditolak** |
| 4c | Petugas Aset | **Batalkan** beralasan sebelum dikonfirmasi → kunci dilepas, penempatan tidak berubah. Status **Dibatalkan** |
| 5 | Siapa pun berhak baca | **Berita Acara Penyerahan** (PDF) untuk penyerahan yang sudah dikonfirmasi |

Contoh dari uji: `AST-PNY-000006` menyerahkan 3 kursi ke Instalasi Rawat Inap / Ruang Uji U9KOL A dengan penanggung
jawab Dimas Kurniawan. Dimas mengonfirmasi → linimasa tiap kursi: "Ditempatkan — Belum Ditempatkan → Instalasi Rawat
Inap / Ruang Uji U9KOL A". Lalu `AST-PNY-000007` memindahkan satu kursi ke Ruang B di unit yang sama → linimasa "Pindah
Dalam Unit"; riwayat penempatan kursi itu 2 baris (Ruang A selesai, Ruang B berjalan).

**Siapa "penerima yang dituju".** Pengguna yang memegang penugasan manajerial aktif di unit tujuan, **atau** yang
akunnya terhubung ke pegawai penanggung jawab. Tidak ada pengecualian berbasis peran: superadmin yang bukan keduanya
ditolak 403 (`AST-VAL-REG-025`).

**Jalur tidak normal**

| Kondisi | Hasil | Kode |
|---|---|---|
| Tujuan Koreksi Opname / Pascapemulihan lewat API | 400 — dibuat otomatis oleh prosesnya | `AST-REG-FIELD` |
| 0 atau lebih dari 200 aset; tujuan tidak lengkap; catatan > 1000 karakter | 400 | `AST-REG-FIELD` |
| Aset tidak ada / milik badan hukum lain | 404 | `AST-REG-NOT-FOUND` |
| Aset di luar unit petugas (tanpa `ReadAllUnits`) | 403 | `AST-VAL-REG-016` |
| Aset belum `Terdaftar` | 409 | `AST-VAL-REG-020` |
| Aset `Hilang` | 409 | `AST-VAL-REG-021` |
| Penempatan awal untuk aset yang sudah ditempatkan; pindah ke unit lain; "pindah" untuk aset belum ditempatkan | 409 | `AST-VAL-REG-022` |
| Unit/ruangan tidak aktif, ruangan bukan milik unit tujuan, pegawai tidak aktif | 400 | `AST-VAL-REG-023` |
| Aset sedang dalam proses lain (mis. penyerahan lain) | 409, menyebut nomor aset dan nama proses | `AST-VAL-REG-024` |
| Bukan penerima yang dituju saat konfirmasi/tolak | 403 | `AST-VAL-REG-025` |
| Unit asal/tujuan sedang opname | 409 (titik panggil siap; isi `BE-AST-021`) | `AST-VAL-REG-026` |
| Alasan kosong saat tolak/batal | 400 | `AST-VAL-REG-010` |
| Aksi atas penyerahan yang sudah diputuskan; berita acara sebelum konfirmasi | 409 | `AST-PLC-STATUS` |
| Penyerahan tidak ada | 404 | `AST-PLC-NOT-FOUND` |

Semua pemeriksaan aset dilakukan **sebelum** nomor `AST-PNY` dialokasikan, sehingga percobaan yang ditolak tidak
menghabiskan nomor.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`AGENTS.md`; `docs/engineering/BACKEND_ENGINEERING_CONTRACT.md`, registry; `rules/backend/*`, standar endpoint
transaksi; kontrak 2.9–2.10, `state-transition-matrix` §4, `validation-matrix` §3, `permission-audit-matrix`,
`acceptance-test-matrix`, `00-interview-decisions` (`AST-DEC-044`, `046`), `02-backend-architecture`; model
`AstAssetHandover`, `AstAssetHandoverItem`, `AstAssetPlacement` dan configuration-nya (indeks unik penempatan
berjalan `EndAt IS NULL`); `AssetProcessLockService`, `AssetHistoryService`, `AssetNumberingService`,
`AssetActorResolver`, `AssetRecordService.Approval.cs` (pola transaksi dan `FOR UPDATE`);
`PermissionRegistryDescriptor` dan preseden `ControllerName` bersama (`BloodOrder`); `ApplicationUser`
(`DisplayName`, `PrimaryPosition`), `MstAssetLocation.OrganizationUnitId`, `MstEmployee`.

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Register/Services/AssetPlacementService.cs` | Baru — serahkan, konfirmasi, tolak, batal; `ApplyPlacementAsync` publik untuk pergerakan; validasi `020..025`; kunci baris `FOR UPDATE` berurutan Id |
| `Register/Services/AssetPlacementService.Query.cs` | Baru — daftar/rincian/ringkasan/metadata penyerahan dengan cakupan unit, penempatan berjalan, riwayat penempatan, data berita acara |
| `Register/Services/AssetStockTakeGate.cs` | Baru — titik panggil `AST-VAL-REG-026` (`FindUnitsInStockTakeAsync` mengembalikan kosong sampai `BE-AST-021`) |
| `Register/Services/AssetLabelDocumentService.Handover.cs` | Baru — PDF Berita Acara Penyerahan (QuestPDF) |
| `Register/Services/AssetLabelDocumentService.cs` | Diubah — `partial` |
| `Register/Controllers/AssetHandoversController.cs` | Baru — 9 endpoint 2.9 + `filters/metadata`, `summary` |
| `Register/Controllers/AssetPlacementsController.cs` | Baru — 2 endpoint 2.10 |
| `Register/DTOs/AssetPlacementDtos.cs` | Baru — request, query, response, `AssetPlacementTarget` |
| `Register/Services/AssetManagementException.cs` | Diubah — kode `020..023`, `025`, `026`, `AST-PLC-*` dan pesannya (persis `validation-matrix`) |
| `Register/Services/AssetRecordService.cs` | Diubah — label `HandoverPurpose`, `HandoverStatus` |
| `AssetManagementServiceCollectionExtensions.cs` | Diubah — registrasi `AssetStockTakeGate`, `AssetPlacementService` |

Nol baris komentar.

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 9 endpoint 2.9–2.10 tersedia. Delta: (1) `GET /handovers/filters/metadata` dan `GET /handovers/summary` ditambahkan sesuai standar aggregate ber-lifecycle (dipakai layar `FE-AST-006` untuk saringan dan hitungan "menunggu konfirmasi Anda"); (2) `AssetPlacementPagedQuery` + `awaitingMyConfirmation`, `assetId`, `from`, `to`, `search`; (3) `POST /handovers` hanya menerima tujuan `PenempatanAwal`/`PindahDalamUnit`; `KoreksiOpname`/`PascaPemulihan` dibuat oleh proses inventarisasi/pemulihan lewat service; (4) kode galat tambahan `AST-PLC-NOT-FOUND`, `AST-PLC-STATUS`; (5) `/status-history` dan `PUT /{id}` tidak dibuat — penyerahan tidak dapat disunting, dan setiap langkah tercatat di kolom penyerahan serta linimasa aset |
| Database | `NOT APPLICABLE` untuk schema. Data baru ditulis ke tabel yang sudah ada; penutupan penempatan lama disimpan sebelum penempatan baru dibuka (indeks unik terfilter) dalam satu transaksi |
| Keamanan/Auth | Resource `AssetPlacement` (`Read`, `Create`, `Confirm`, `Cancel`) pada dua controller dengan `AccessController` identik (preseden `BloodOrder`). Cakupan data: tanpa `ReadAllUnits`, pengguna melihat penyerahan ke unitnya, untuk dirinya sebagai penanggung jawab, atau yang ia serahkan. Penerima ditentukan dari data HR, bukan nama peran |

---

## 4. Dokumentasi endpoint

#### Corporate / Asset Management / Register / Asset Handover

Base URL: `api/v1/corporate/asset-management/register/handovers`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Saringan status/tujuan, parameter kueri | `AssetPlacement : Read` |
| `GET` | `/summary` | Jumlah per status + menunggu konfirmasi Anda | `AssetPlacement : Read` |
| `GET` | `/` | Daftar penyerahan | `AssetPlacement : Read` |
| `GET` | `/{id}` | Rincian beserta butir aset, nama/jabatan tiap langkah, `availableActions` | `AssetPlacement : Read` |
| `POST` | `/` | Menyerahkan aset | `AssetPlacement : Create` |
| `POST` | `/{id}/confirm` | Penerima mengonfirmasi; penempatan berlaku | `AssetPlacement : Confirm` |
| `POST` | `/{id}/reject` | Penerima menolak beserta alasan | `AssetPlacement : Confirm` |
| `POST` | `/{id}/cancel` | Petugas membatalkan sebelum dikonfirmasi | `AssetPlacement : Cancel` |
| `GET` | `/{id}/document` | PDF Berita Acara Penyerahan | `AssetPlacement : Read` |

#### Corporate / Asset Management / Register / Asset Placement

Base URL: `api/v1/corporate/asset-management/register/placements`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/` | Penempatan berjalan per unit/ruangan/penanggung jawab | `AssetPlacement : Read` |
| `GET` | `/history` | Riwayat penempatan (per aset, rentang tanggal) | `AssetPlacement : Read` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE Strict `WorkingTree` | 11 berkas, `VIOLATION: 0`, `REVIEW: 0` (dua kali: sebelum dan sesudah perbaikan urutan kunci) | `PASS` | Keluaran perintah |
| Grep komentar | 0 baris | `PASS` | — |
| `dotnet build` | Dijalankan Rizki dan berhasil; DLL terakhir 2026-11-17 14:38:32, sesudah suntingan terakhir 14:33:24 | `PASS` | `bin/Debug/net9.0` |
| Uji runtime putaran 1 (DLL 14:05) | 32/32; ditemukan nomor `AST-PNY-000002` terbuang oleh percobaan `024` | `NEW ERROR` → diperbaiki | `ast009-runtime.json` (tag `U94W3`) |
| Uji runtime putaran 2 (DLL 14:38) | **33/33** termasuk cek nomor berurutan (`N-01`) | `PASS` | `test-with-agy/asset-management/be-ast-009-20261117/ast009-runtime.json` (tag `U9KOL`) |
| Berita acara dibuka di Edge | Kop, nomor, tujuan, 3 butir aset, blok penyerah/penerima dengan nama, jabatan, waktu WIB | `PASS` | `berita-acara-pratinjau.png`, `berita-acara-AST-PNY-000001.pdf`, `…000006.pdf` |

Aktor uji: superadmin uji (penyerah; **bukan** penerima), akun Perawat (Dimas Kurniawan, terhubung ke pegawai =
penanggung jawab yang dituju), akun penyetuju aset (tanpa izin `AssetPlacement`), LOKET.

| Kriteria | Langkah uji | Hasil |
|---|---|---|
| AC1 matriks §4 | Serahkan → Konfirmasi; Serahkan → Batalkan; Serahkan → Tolak terima; aksi ulang sesudah diputuskan | Diserahkan → Dikonfirmasi / Dibatalkan / Ditolak; aksi ulang 409 `AST-PLC-STATUS`; tolak/batal tanpa alasan 400 `010` |
| AC2 penempatan hanya saat konfirmasi | Sesudah serahkan: aset masih Belum Ditempatkan; superadmin konfirmasi 403 `025`; Dimas konfirmasi | Unit Instalasi Rawat Inap, ruang U9KOL A, PJ Dimas, `Ditempatkan`; linimasa "Ditempatkan" bernomor penyerahan |
| AC3 `PindahDalamUnit` | Ke unit lain; aset belum ditempatkan; penempatan awal untuk aset sudah ditempatkan; pindah ruangan di unit sama | Tiga yang pertama 409 `022`; yang terakhir berlaku, riwayat 2 baris (lama selesai) |
| AC4 kunci proses | Sesudah serahkan `activeProcessKind=1`; serahkan ulang aset sama; sesudah konfirmasi/batal/tolak | 409 `024` "Aset AST-MBL-000033 sedang dalam proses penyerahan…"; kunci kembali 0 |
| AC5 berita acara | `/document` sebelum konfirmasi; sesudah konfirmasi | 409; sesudah: PDF 43.717 byte; rincian memuat nama + jabatan + waktu penerima ("Dimas Kurniawan, Perawat IGD") |
| AC6 titik panggil opname | `AssetStockTakeGate` dipanggil untuk unit asal dan tujuan sebelum nomor | Terpanggil di setiap penyerahan; mengembalikan "tidak ada unit opname" sampai `BE-AST-021` |

`AT-REG-04` (serahkan lalu konfirmasi; 3 kursi, bukan 5 kursi roda) dan `AT-REG-05` lulus. `AT-REG-36` lulus untuk
kunci penyerahan; varian "aset sedang diusulkan dihapus" menunggu `penghapusan`. `AT-REG-37` lulus dengan aktor
bukan-penerima; varian "Kepala Unit lain" belum teruji karena DB dev tidak punya penugasan manajerial aktif.
`AT-REG-38` menunggu `BE-AST-021`.

Uji manual: `NOT APPLICABLE` (endpoint diuji langsung; layar `FE-AST-006`).

`AUTOMATED TEST: SKIPPED (opsional) — kebijakan tanpa unit test`.

**Data yang ditulis ke `QuilvianNewDevRizki`** (lewat API, atas persetujuan Rizki untuk uji modul Aset): 6 lokasi
uji (`U94W3-A/B/C`, `U9KOL-A/B/C`; A dan B di Instalasi Rawat Inap, C di Instalasi Gawat Darurat); penyerahan
`AST-PNY-000001` sampai `000009` (4 dikonfirmasi, 2 ditolak, 2 dibatalkan; `000002` tidak terpakai karena cacat
putaran 1); kursi `AST-MBL-000031..000036` kini `Ditempatkan` di Instalasi Rawat Inap dengan penanggung jawab Dimas
Kurniawan (`000033` dan `000036` sudah pindah ke ruang B masing-masing putaran).

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| 1. Matriks state §4 lengkap | Terpenuhi | `CreateHandoverAsync`, `ConfirmAsync`, `RejectAsync`, `CancelAsync`; uji AC1 |
| 2. Penempatan berubah hanya saat konfirmasi penerima yang dituju | Terpenuhi | `EnsureRecipient` (kepala unit tujuan / pegawai PJ) + `ApplyPlacementAsync` hanya di `ConfirmAsync`; uji AC2 |
| 3. `PindahDalamUnit` hanya di unit yang sama | Terpenuhi | `EnsureHandoverEligible`; uji AC3 |
| 4. Kunci proses dipasang/dilepas | Terpenuhi | `AssetProcessLockService.Acquire`/`Release` di keempat transisi; uji AC4 |
| 5. Berita acara memuat nama, jabatan, waktu (`AST-DEC-046`) | Terpenuhi | `RenderHandoverReport`; PDF dirender dan diperiksa |
| 6. Pemeriksaan unit sedang opname disiapkan sebagai titik panggil untuk `BE-AST-021` | Terpenuhi | `AssetStockTakeGate.EnsureUnitsNotInStockTakeAsync` dipanggil dari `CreateHandoverAsync` |
| `DoD-BE` | Terpenuhi | QBE, build, runtime, laporan, roadmap, traceability |

Rekap: **6 dari 6 kriteria terpenuhi** → `✅`.

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Selama pengerjaan, backend dua kali (start 14:06 dan 14:20) memakai ±15 inti CPU tanpa permintaan apa pun: 40 thread sibuk merata, tanpa log, koneksi DB habis waktu. Start 14:29 dan 14:39 normal (CPU 0, juga sesudah 33 skenario uji). Merge integration tidak menambah atau mengubah hosted service, dan kode task ini tidak berjalan tanpa permintaan; penyebab belum diketahui karena alat diagnosis (`dotnet-trace`/`dotnet-dump`) tidak tersedia. Opsi `--Runtime:Role=Web` tidak terbaca pada cara backend dijalankan (log tetap `All`) |
| Masalah yang diketahui | (1) Penyerah tanpa jabatan utama (superadmin) tercetak "Jabatan tidak tercatat" di berita acara. (2) `AssetStockTakeGate` belum memeriksa apa pun sampai `BE-AST-021`. (3) Varian `AT-REG-36` (penghapusan) dan `AT-REG-37` (Kepala Unit lain) belum teruji, lihat bagian 5 |
| Risiko tersisa | Penerima ditentukan dari data HR (penugasan manajerial, akun-pegawai); data HR yang salah membuat penerima tidak dapat mengonfirmasi (`AST-UNK-004`) |
| Perubahan sampingan | `NONE` |
| Interupsi | Backend tidak responsif dua kali (peringatan di atas); dipulihkan dengan restart oleh Rizki |
| Status Git | 4 berkas diubah, 7 berkas baru, + dokumen; belum di-commit |
| Langkah berikutnya | `FE-AST-006` (penempatan dan berita acara penyerahan) |
