# MCU — Persetujuan Lintas Modul dan Pembuktian Teknis (`MCU-EVD-002`)

| Field | Nilai |
| --- | --- |
| Evidence ID | `MCU-EVD-002` |
| Tanggal | 2026-10-21 |
| Disampaikan oleh | Yoga Aji Pratama, Product/Domain Owner MCU (`MCU-DEC-002`) |
| SHA backend | `4d3e4f98aca0d9cbf6d5e0b62070d94d4760cfec` (branch `yoga`, working tree tidak bersih — sama dengan snapshot manifest) |
| SHA frontend | `7fb1e1d3eb18df33a893cf2cb1a5cc7c6914acb9` (branch `YogaV2`) — tidak disentuh |
| Penulisan basis data | **Tidak ada.** Seluruh uji memakai EF Core InMemory; berkas uji sementara di folder `Areas/` sudah dihapus |

## 1. Persetujuan lintas modul

Ketiga persetujuan di bawah **disampaikan** oleh Yoga Aji Pratama pada sesi 2026-10-21, dengan pernyataan
bahwa izin pemilik masing-masing sudah diperoleh. Bukti tertulis dari setiap pemilik tidak dilampirkan di
sini; pola pencatatannya mengikuti preseden baris registry `Pat` / Membership 2026-10-13.

| Gerbang | Isi yang disetujui | Pemilik pemberi izin | Akibat |
| --- | --- | --- | --- |
| `MCU-GOV-001` | Baris registry `HealthServices` / `MedicalCheckupManagement / Medical Check-Up` / `BUSINESS DOMAIN / MODULE` / `Mcu` / `ACTIVE` | Muhammad Hamzah, pemilik registry | Baris ditambahkan ke `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` beserta catatan lifecycle 2026-10-21. Penghalang QBE-MOD-002 atas model `Mcu*` pertama dicabut. Bukan wewenang migration, database, maupun deployment |
| `MCU-COORD-002`, `MCU-COORD-008` | Kolom `CoveringMcuOrderId` pada `LabOrder` dan `RadOrder`; penahanan fakta tagih Lab/Rad untuk pesanan bertanda paket (`MCU-DEC-022`); dokter MCU yang ditetapkan sebagai `InstructingDoctorId` dan penerima nilai kritis pesanan paket (`MCU-DEC-032`) | Pemilik `laboratorium` dan `radiologi` | Membuka `MVP-0` (kolom) dan sisi Lab/Rad `MVP-2`. `LAB-DEC-164` dipersempit untuk pesanan bertanda paket; pencatatannya di blueprint Laboratorium tetap pekerjaan terpisah |
| Perluasan `EncounterIntakeService` | Jalur pembuatan kunjungan `MedicalCheckup` berantrean skrining untuk check-in MCU (`02-backend-architecture.md` bagian 7) | Pemilik Registrasi | Membuka `MVP-1` |

### Bukti checker registry

Checker `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Path <berkas>` dijalankan atas dua model uji sementara
ber-`[Table(...)]` (atribut itu yang membuat checker menilai kelas sebagai entity persisted):

| Model uji sementara | QBE-MOD-002 | Keterangan |
| --- | --- | --- |
| `Areas/HealthServices/MedicalCheckupManagement/Models/McuPackage.cs` | **Tidak muncul** | Dua pelanggaran lain muncul karena model uji sengaja tanpa base model dan configuration — wajar |
| `Areas/HealthServices/ZzzUjiKontrolManagement/Models/ZzzPackage.cs` (kontrol negatif) | **Muncul** — *"Obtain registry decision; do not infer a prefix."* | Membuktikan checker memang menilai QBE-MOD-002 pada jalur ini |

Kedua berkas dihapus sesudah uji; `git status` tidak menyisakan berkas di `Areas/`. Percobaan pertama tanpa
`[Table]` lolos pada **kedua** model dan karena itu tidak dihitung sebagai bukti.

Salinan registry canonical di `QuilvianEngineeringSkills` belum memuat baris `Mcu` (dan juga belum memuat
baris `Pat` / Membership 2026-10-13). Penyelarasannya milik pemegang registry.

## 2. Pembuktian teknis

Harness: project uji sementara (xUnit, `Microsoft.EntityFrameworkCore.InMemory` `9.0.18`) yang mereferensikan
`QuilvianSystemBackend.dll` hasil build 2026-10-20 15.53 — lebih baru dari seluruh source yang berubah di
working tree. Dibangun dengan `-p:RunAnalyzers=False -p:BuildProjectReferences=false`. Hasil: **6 uji, 6 lolos**.
Harness berada di scratchpad sesi dan tidak ikut repository; kode service yang diuji dibaca langsung dari source.

### 2.1 `ARC-TECH-01` — Lab membatalkan satu pemeriksaan dalam pesanan berisi beberapa pemeriksaan → **TERBUKTI DIDUKUNG**

Jalur yang sudah ada: `POST api/v1/health-services/laboratory-management/lab-examinations/{id}/cancel`
(`LabExaminationController.Cancel`, izin `LabExamination : Update`) →
`LabExaminationService.CancelAsync` (`Areas/HealthServices/LaboratoryManagement/Services/LabExaminationService.cs:260`).

| Uji | Keadaan awal | Hasil |
| --- | --- | --- |
| Batal satu dari dua | Pesanan PK `InProcess`, satu wadah EDTA `Accepted`, Darah Lengkap dan LED `ChargeEligible` | DL → `Cancelled` (`IsCancel = true`, `Version` 0 → 1); **LED tetap `ChargeEligible`**; pesanan tetap `InProcess`; wadah tetap `Accepted`; satu baris `LabTransitionHistory` `Examination.Cancel ChargeEligible → Cancelled` beralasan, menunjuk DL |
| Hasil sudah dirilis | DL ber-`ReleasedAt` | Ditolak `LabExaminationValidationException` (`422`): *"Pemeriksaan yang hasilnya sudah dirilis tidak dapat dibatalkan. Hasilnya hanya dapat diperbaiki lewat koreksi."* |
| Alasan kosong | Alasan berisi spasi | Ditolak `422`: *"Alasan pembatalan wajib diisi."* |

**Akibat pada desain MCU:**

1. `LabOrderService` **tidak perlu** ditambah jalur batal per pemeriksaan. `McuExaminationService` memanggil
   `LabExaminationService.CancelAsync` untuk setiap `LabExamination` milik komponen yang dibatalkan.
2. Penjaga Lab berbasis **`ReleasedAt`**, bukan *tervalidasi*. Pemeriksaan yang sudah divalidasi tetapi belum
   dirilis masih dapat dibatalkan Lab. Pesan `409` *"Pemeriksaan Lab sudah divalidasi dan tidak dapat
   dibatalkan."* pada `state-transition-matrix.md` karena itu menjadi penjaga **MCU sendiri** (cek
   `ValidatedAt` sebelum memanggil Lab), dan penolakan `422` Lab untuk hasil yang sudah dirilis diterjemahkan
   MCU menjadi `409` yang sama.
3. Pembatalan semua pemeriksaan tidak menutup pesanannya. Batal penuh order MCU (`MCU-DEC-021`) tetap memakai
   jalur batal pesanan `POST lab-orders/{id}/cancel`.

### 2.2 `MCU-UNK-04` — Billing menerima refund nominal sebagian dari satu baris paket → **TERBUKTI TIDAK DIDUKUNG**

Jalur yang diuji: `BillingRefundService.CreateAsync`
(`Areas/HealthServices/BillingManagement/Billing/Services/BillingRefundService.cs:108`).

Kasus: tagihan berisi satu baris *Paket MCU Basic* Rp 350.000, lunas tunai. Dokter MCU membatalkan Thorax
yang bernilai pengembalian Rp 150.000 (`MCU-DEC-015`).

| Uji | Hasil | Sebab di kode |
| --- | --- | --- |
| Refund Rp 150.000 atas baris paket | Case tercatat **`RequestedAmount` Rp 350.000**, satu baris refund Rp 350.000 | `RequestedAmount` dari request tidak dibaca. Nominal selalu `SUM(Quantity × UnitPrice)` baris terpilih (`:171`) dan tidak masuk hash payload (`:730`) |
| Komponen kedua (Audiometri Rp 50.000) selagi case pertama belum selesai | Ditolak `BillingRefundConflictException`: *"Invoice ini masih memiliki refund case aktif; selesaikan case yang sudah ada terlebih dahulu."* `GetBillingRefundableItemsAsync` menampilkan `RefundableAmount` baris paket = Rp 0 | Hanya satu case aktif per invoice per kategori (`:191`); daftar refundable menandai baris sebagai sudah direfund utuh begitu muncul di case mana pun |
| Komponen kedua sesudah case pertama `EXECUTED` | **Diterima** sebesar Rp 350.000 lagi — total diajukan atas baris Rp 350.000 menjadi **Rp 700.000** | `CreateAsync` tidak memeriksa baris yang sudah pernah direfund; hanya layar refundable yang memeriksanya |

**Uji ketiga menunjukkan cacat Billing yang berdiri sendiri, terlepas dari MCU.** Satu baris tagihan
dapat direfund lebih dari nilainya lewat dua case berurutan. Cacat ini dilaporkan kepada pemilik
`billing-kasir`; perbaikannya di luar wewenang modul MCU.

**Akibat pada desain MCU:** refund komponen (`FR-MCU-034`, `AT-MCU-30`) tidak dapat dibangun di atas jalur
refund yang ada. Pemilik produk memilih **memperluas Billing** (`MCU-DEC-043`).

## 3. Keputusan pemilik pada sesi ini

| ID | Ringkasan | Dicatat di |
| --- | --- | --- |
| `MCU-DEC-041` | Ke-18 aturan berlabel **DES** di `validation-matrix.md` disahkan apa adanya | `00-interview-decisions.md` r5 |
| `MCU-DEC-042` | Menutup `DES-MCU-001`: dokter MCU **jaga harian** per tanggal di jadwal; check-in kiosk memakai dokter jaga hari itu, tanpa dokter jaga kiosk menolak dan mengarahkan ke admisi | `00-interview-decisions.md` r5 |
| `MCU-DEC-043` | Menutup `MCU-UNK-04`: refund komponen lewat **perluasan Billing** (nominal sebagian per baris, akumulasi per baris, penutupan celah refund ganda); butuh persetujuan pemilik `billing-kasir`; `MVP-2` tertahan sampai tersedia | `00-interview-decisions.md` r5 |
| Approval blueprint | `MCU-BP-001` revision 1 dan kontrak `v1` disetujui | `blueprint-manifest.md` |

## 4. Tambahan sesudah roadmap (2026-10-21)

| Hal | Isi | Pemberi persetujuan |
| --- | --- | --- |
| Roadmap | `MCU-RM-BE-001` r1, `MCU-RM-FE-001` r1, dan traceability r1 disetujui | Yoga Aji Pratama |
| `MCU-COORD-003` | Perluasan refund Billing sesuai `MCU-DEC-043` — nominal sebagian ≤ sisa per baris, akumulasi refund per baris, tolak melebihi nilai baris — termasuk penutupan celah refund ganda pada bagian 2.2 | Yasmina, pemilik `billing-kasir`; disampaikan Yoga Aji Pratama dengan pernyataan bahwa persetujuan sudah diperoleh |

Bentuk teknis akumulasi per baris dan perlakuan peserta perusahaan (potong tagihan kolektif) ditetapkan
pemilik Billing saat `BE-MCU-026` dikerjakan.
