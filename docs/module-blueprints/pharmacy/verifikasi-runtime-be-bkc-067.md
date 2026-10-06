# Verifikasi runtime `BE-BKC-067` — penerbitan financial clearance

Dijalankan 1 Oktober 2026 pada `localhost / QuilvianNewDevIkbalFr`, seluruhnya lewat HTTP endpoint
existing. Tanpa `psql`, tanpa insert/update tabel, tanpa fixture `BilPrescriptionClearanceHandoff`,
tanpa satu baris perubahan source Billing.

**Hasil: rantai target tidak dapat diselesaikan.** Tiga blocker runtime nyata, seluruhnya di luar
modul Farmasi/Operasi/Gizi. `BE-BKC-067` **belum layak** ✅ Selesai.

## Endpoint yang dipakai

| Keperluan | Endpoint | Hasil |
|---|---|---|
| Autentikasi | `POST /api/v1/Auth/login` | 200, cookie `quilvian_access_token` |
| Baca resep + pemicu consumer | `GET .../pharmacy-management/prescriptions/{id}` | 200 |
| Daftar invoice | `GET .../billing/invoices` | 200, 5 invoice, semuanya 0 item |
| Masukkan charge obat | `POST .../billing/invoices/from-source` | **422** (6 kali) |
| Masukkan charge non-obat | `POST .../billing/invoices/other-charges` | 422 master data |
| Masukkan charge tindakan | `POST .../billing/invoices/from-source` | **500** drift skema |
| Buat kategori tarif | `POST .../master-data/tariff-categories` | 200 |
| Buat settlement | `POST .../billing/patient-funds/settlements` | **422** |
| Riwayat settlement | `GET .../patient-funds/invoices/{id}/settlements` | 200, kosong |
| Surat menggantung | `GET .../consumer-handoffs/pending?handoffType=PRESCRIPTION` | 200, 17 (semua fixture sesi lalu) |
| Metode bayar | `GET .../master-data/payment-methods` | 200, 7 metode |

## Subjek uji

| Hal | Nilai |
|---|---|
| Resep | `RX-20261001-00003` — `4e4c6351-73aa-4cdf-84ad-f4f1c2936b92` |
| Encounter | `289e87d6-0bf6-462e-9453-b22edbc367a3` |
| Invoice | `INV-UJI-003` — `dddd0000-0000-0000-0000-000000000001` |

Resep ini lahir dari alur nyata hari ini, sudah difinalkan (`prescriptionStatus = 2`), dan berada
tepat pada tahap yang dituju.

## State sebelum pembayaran

```
fulfillmentStatus   2   (Menunggu Pembayaran)
prescriptionStatus  2   (Submitted)
paymentStatus       1   (NotBilled)
totalItemCount      1
financialClearance  clearanceStatus=UNKNOWN, financialOutcome=null, isCleared=false,
                    financialVersion=0, holdReasonCode=PHA_CLR_UNKNOWN
```

`GET prescriptions/{id}` memanggil `ConsumeForPrescriptionAsync` di dalam prosesnya, jadi consumer
Farmasi **memang berjalan** — ia hanya tidak menemukan surat apa pun. Jalur consumer terbukti hidup;
yang tidak ada adalah suratnya.

## Blocker 1 — kontrak charge obat mengunci dirinya sendiri (utama)

`BillingChargeSourceAdapter.cs:39` menetapkan `["PHARMACY"] = Policy(["DISPENSED"], [], [])`, dan
baris 87 menolak eksplisit setiap status lain dengan *"Jumlah obat yang diserahkan belum final."*

Enam status pra-penyerahan yang jujur dicoba, semuanya **422**:

| `sourceStatus` | HTTP | Pesan |
|---|---|---|
| `SUBMITTED` | 422 | Jumlah obat yang diserahkan belum final. |
| `PRESCRIBED` | 422 | idem |
| `FINALIZED` | 422 | idem |
| `READY` | 422 | idem |
| `VERIFIED` | 422 | idem |
| `PREPARED` | 422 | idem |

Hanya `DISPENSED` yang diterima. Tetapi menyatakan `DISPENSED` pada resep tahap 2 adalah pernyataan
palsu tentang fakta klinis — obatnya belum diserahkan — sehingga tidak dilakukan.

Akibatnya lingkaran tertutup:

```
item invoice PHARMACY  --butuh-->  resep DISPENSED (tahap 9)
resep DISPENSED        --butuh-->  clearance sah          (PrescriptionDispensingService.cs:629)
clearance sah          --butuh-->  surat CLEARED
surat CLEARED          --butuh-->  item invoice PHARMACY  (BilConsumerHandoffService.cs:303)
```

Telaah pun ikut terkunci: `PrescriptionReviewService.cs:65` menuntut clearance untuk tahap 4 ke 5.

Tidak ada jalan keluar lewat `specificPrescriptionId`: `BilPrescriptionClearanceRecoveryService.cs:43`
juga mulai dari `BilInvoiceItems` ber-`SourceDomain = "PHARMACY"`.

**Dan tidak ada produsernya.** Penelusuran seluruh `Areas/` menemukan `UpsertChargeRequest` hanya
dirujuk 4 berkas, semuanya di dalam Billing sendiri. Modul Farmasi **tidak memiliki satu pun kode**
yang mengirim charge obat ke Billing. Jadi bukan soal urutan yang salah dipanggil — pengirimnya
belum pernah dibuat.

## Blocker 2 — drift skema menutup seluruh jalur charge

`POST from-source` domain `PROCEDURE` status `PERFORMED` (kategori tarif dibuat lewat endpoint master
data, `TC-RSMMC-00001`) menghasilkan **500**:

```
Npgsql.PostgresException 42703: column b1.DoctorDiscountMemoFile does not exist
  BillingInvoiceService.UpsertChargeAsync ... line 1497
```

Kolomnya ada pada model, tidak ada pada basis data. Pemiliknya tabel `BilDiscountApplication`, dan
yang menambahkannya migration
**`20260924083000_AddBillingManagementRevisionsDoctorMemoAndRefundCategory`** — belum terpasang.

Angka "199 dari 218 Pending" yang sempat saya laporkan **tidak dapat dipakai**: `dotnet ef migrations
list` dijalankan atas build ber-`SkipMigrationMetadata=true`, yang mengecualikan berkas
`*.Designer.cs`. Migration tanpa atribut `[Migration]` inline karena itu tidak terlihat EF sama
sekali — termasuk migration penghalang di atas, yang memang tidak muncul pada daftar itu. Daftarnya
jadi tidak mewakili keadaan basis data.

Yang tetap pasti dan terbukti langsung dari PostgreSQL: **kolom itu tidak ada**. Satu DDL yang
menghalangi:

```
ALTER TABLE public."BilDiscountApplication"
  ADD COLUMN "DoctorDiscountMemoFile" character varying(500) NULL;
```

Konteksnya tetap blocker rantai migration yang tercatat di
`docs/engineering/blocker-rantai-migration.md`: basis data dibangun dari baseline hasil squash,
sehingga tabel Billing ada tetapi kolom-kolom sesudahnya tidak.

Ketiga pintu charge — `UpsertChargeAsync`, `AddCatalogChargeAsync` (baris 1136/1160), dan
`AddOtherChargeAsync` (baris 1225) — bermuara ke query yang sama di baris 1497. Karena itu
**tidak ada charge jenis apa pun yang dapat masuk ke invoice mana pun** pada basis data ini.

Turunannya: `POST settlements` menolak dengan **422 "Invoice belum memiliki hasil perhitungan
terkini."** Endpoint settlement sendiri sehat — ia memvalidasi `Purpose` lebih dulu, lalu berhenti
karena invoicenya nol item. Tidak ada tender yang dapat dicatat.

## Blocker 3 — master data cara bayar tidak bertanda (menyangkut `BIL-AT-140`)

Tujuh metode bayar ada, tetapi **ketujuhnya** ber-`isCash`, `isInsurance`, dan `isCompanyGuarantor`
seluruhnya `false` — termasuk `TUNAI`, `JAMINAN` (Jaminan Asuransi), dan `PENJAMIN` (Jaminan
Perusahaan).

Penurunan hasil finansial membaca tanda-tanda itu. Dengan master data sekarang, setiap pembayaran
akan diturunkan menjadi `PAID`, dan `INSURANCE_APPROVED` tidak akan pernah terjadi walaupun kasir
memilih Jaminan Asuransi. Skenario tunai kebetulan benar; skenario asuransi tidak dapat diuji dan
secara diam-diam akan salah.

## Hasil `BIL-AT-137` sampai `BIL-AT-140`

| Uji | Yang dituntut | Hasil runtime |
|---|---|---|
| `BIL-AT-137` | biaya non-obat tidak mencabut clearance | **TIDAK DAPAT DIUJI** — tidak ada resep yang pernah `CLEARED`, dan charge non-obat pun tertolak 500 (Blocker 2) |
| `BIL-AT-138` | koreksi harga obat naik mencabut clearance | **TIDAK DAPAT DIUJI** — item obat tidak pernah dapat masuk (Blocker 1) |
| `BIL-AT-139` | pembalikan tender mencabut seluruh resep | **TIDAK DAPAT DIUJI** — tidak ada tender yang dapat dicatat (Blocker 2) |
| `BIL-AT-140` | tender bercampur menghasilkan penjaminan | **TIDAK DAPAT DIUJI, dan diperkirakan gagal** — tidak ada metode bayar bertanda asuransi (Blocker 3) |
| `BIL-AT-135` | settlement menerbitkan `CLEARED` | **TIDAK DAPAT DIUJI** — settlement tertolak 422 |

Keempatnya tetap berstatus "VERIFIED (Logic)" seperti pada laporan task aslinya, yaitu terbaca benar
pada source tetapi belum pernah dijalankan. Verifikasi runtime yang diminta roadmap **belum
terpenuhi**, dan sebabnya bukan source `BE-BKC-067`.

## State resep sesudah percobaan

Tidak berubah, sebagaimana seharusnya:

```
fulfillmentStatus   2        (tetap)
prescriptionStatus  2        (tetap)
financialVersion    0        (tetap)
clearanceStatus     UNKNOWN  (tetap)
```

Tidak ada lompatan maju, tidak ada lompatan mundur, dan tidak ada data klinis yang tersentuh.

## Yang terbukti benar

Tiga hal ikut terbukti sehat dan layak dicatat:

1. **Consumer Farmasi hidup dan fail-closed.** Dipanggil nyata lewat `GET prescriptions/{id}`,
   menghasilkan `PHA_CLR_UNKNOWN` dan menahan resep, bukan meloloskannya.
2. **Jalur baca surat hidup.** `GET consumer-handoffs/pending` membaca
   `BilPrescriptionClearanceHandoffs` dan mengembalikan 17 baris — seluruhnya fixture sesi
   sebelumnya, tidak dipakai sebagai bukti di sini.
3. **Endpoint settlement dan tender ada dan tervalidasi benar**, berikut frontend kasirnya
   (`billing-settlement-slice.jsx` terdaftar di `store.jsx:155`, dipakai `use-billing-settlement.js`
   lewat `menu-pembayaran-view.jsx`). Yang belum pernah terjadi bukan karena FE tidak tersambung.

## Data uji yang ditinggalkan

| Hal | Identitas | Catatan |
|---|---|---|
| Kategori tarif | `TC-RSMMC-00001` "Tindakan (Uji BKC067)" | dibuat lewat endpoint master data; aman dihapus setelah laporan diterima |

Tidak ada baris lain yang tercipta — semua percobaan charge dan settlement tertolak sebelum menulis.

## Yang dibutuhkan agar `BE-BKC-067` dapat ditutup

Ketiganya milik tim lain, bukan Farmasi:

1. **Keputusan bisnis tim Billing** atas urutan charge obat. Entah `["PHARMACY"]` menerima status
   pra-penyerahan (misalnya saat penyiapan selesai), atau dibuat domain terpisah untuk tagihan obat
   rawat jalan yang dibayar sebelum diserahkan. Ini perubahan kontrak, bukan perbaikan bug.
2. **Produser charge obat di Farmasi** — belum ada sama sekali. Perlu task sendiri, dan menunggu
   keputusan nomor 1 karena status yang dikirimnya ditentukan di sana.
3. **Rantai migration dibereskan** pada lingkungan ini (199 Pending), lalu master data cara bayar
   diberi tanda `IsCash` / `IsInsurance` / `IsCompanyGuarantor`.

---

# Happy path berhasil — 1 Oktober 2026

Setelah circular dependency dipecah dan satu kolom DDL dipasang, rantai penuh terbukti berjalan.

## Perubahan source

| Berkas | Perubahan |
|---|---|
| `BillingChargeSourceAdapter.cs` | policy PHARMACY menerima `SUBMITTED`; guard khusus `DISPENSED` dihapus |
| `PrescriptionBillingChargeProducer.cs` | **baru** — pengirim tagihan obat ke ledger invoice |
| `ConsultationFinalizationService.cs` | memanggil producer sesudah commit, di titik 1 → 2 |
| `Program.cs` | registrasi DI |

Policy lama `Policy(["DISPENSED"], [], [])` menjadi `Policy(["SUBMITTED", "DISPENSED"], [], [])`.

## DDL sempit

```sql
ALTER TABLE public."BilDiscountApplication"
  ADD COLUMN "DoctorDiscountMemoFile" character varying(500) NULL;
```

Sesuai migration `20260924083000_AddBillingManagementRevisionsDoctorMemoAndRefundCategory`.
Diverifikasi sebelum (kolom tidak ada, tabel ada) dan sesudah
(`character varying`, `len=500`, `nullable=YES`). Tanpa `database update`, tanpa kolom lain.

## Data uji yang dibuat lewat API

Master existing tidak disentuh sama sekali.

| Hal | Identitas |
|---|---|
| Kategori tarif | `TC-RSMMC-00001` "Tindakan (Uji BKC067)" |
| Metode bayar | `UJI-PHM-TUNAI` "Tunai Uji Farmasi" · `a3f9e17a-3615-41cc-bd87-c3c1b1a7a131` · `IsCash=true` |
| Register kasir | `UJI-PHM-REG` "Kasir Uji Farmasi" · `489085f4-4cdf-4cbc-ba40-d732a5611a99` |
| Shift kasir | `CSH-20261001-000001` · `166f7b01-198c-4b6b-ae05-81bf44452dd8` |

## Rantai yang terbukti

Subjek: resep `RX-20261001-00002` · `9abbe9b9-8352-4772-9c85-0c2f9d23ad0a`,
encounter `23cbba27-d252-4993-bd83-5a9b49a1c547`.

| Langkah | Endpoint | Hasil |
|---|---|---|
| Charge PHARMACY `SUBMITTED` | `POST invoices/from-source` | **200** · invoice `BIL-20261001-00000002` · `ad3375b1-9b49-45f8-924c-020db7547b92` |
| Invoice dihitung | `POST invoices/{id}/recalculate` | **200** · kalkulasi v1 · Rp30.000 |
| Settlement | `POST patient-funds/settlements` | **201** · `ee999084-bca0-4a41-9a43-868de7690de6` |
| Tender tunai | `POST settlements/{id}/tenders` | **201** · `SUCCEEDED` · `CASH_RECEIVED` · kwitansi `KWS-20261001-0002` |
| Settlement final | `GET settlements/{id}` | `SETTLED` · terbayar Rp30.000 · sisa 0 |
| Invoice | `GET invoices/{id}` | `CLOSED` |
| Surat clearance | `GET consumer-handoffs/pending` | **1 surat** · `a8ce53f4-124e-4e3e-96fb-d55bedc51924` · `ClearanceStatus: CLEARED, Outcome: PAID, Reason: INVOICE_SETTLED` |
| Consumer Farmasi | `GET prescriptions/{id}` | resep naik **tahap 2 → 4** |

Tender tunai terekonsiliasi sendiri di dalam `AddTenderAsync`: karena metodenya ber-`IsCash`,
Billing menyusun sendiri `BillingPaymentProviderResult("cash:…", Succeeded, "CASH_RECEIVED")` lalu
memfinalisasi invoice. Tidak ada payment provider luar yang terlibat.

## State resep sebelum dan sesudah

| | Sebelum | Sesudah |
|---|---|---|
| `fulfillmentStatus` | 2 Menunggu Pembayaran | **4 Dalam Antrean Farmasi** |
| `clearanceStatus` | `UNKNOWN` | `CLEARED` |
| `financialOutcome` | `null` | `PAID` |
| `financialVersion` | 0 | **1** |
| `isCleared` | `false` | `true` |
| `holdReason` | PHA_CLR_UNKNOWN | `null` |
| `paymentStatus` | 1 NotBilled | 5 (akibat finansial dari surat) |
| `prescriptionStatus` | 2 Submitted | 2 Submitted — **tidak berubah** |
| `totalItemCount` | 1 | 1 — **tidak berubah** |
| `totalPrice` | Rp45.000 | Rp45.000 — **tidak berubah** |

Data klinis resep tidak tersentuh. Yang berubah hanya tahap pemenuhan dan keadaan finansialnya.

## Idempotensi

**Charge.** Tiga kali kirim pada invoice pertama menghasilkan satu baris. Kiriman kedua dan ketiga
ditolak **409 "Source version yang sama memiliki isi berbeda."** — penjagaan `SourceVersion` plus
payload hash menolak duplikat, bukan membuat baris kedua. `activeItemCount` tetap 1 dan
`sourceDetailId` tetap satu nilai.

**Consumer.** `GET prescriptions/{id}` dipanggil berkali-kali sesudah promosi; tahap tetap 4,
`financialVersion` tetap 1, dan jumlah surat tetap 1. Tidak ada promosi ganda dan tidak ada surat
kedua.

## Celah yang ditemukan dalam perjalanan

**Tender menggantung tidak punya jalan pulih.** Percobaan tender pertama memakai metode `TUNAI`
existing yang ber-`isCash = false`, sehingga ditolak *"ProviderReference wajib tersedia untuk tender
yang berhasil."* — tetapi **baris tendernya sudah tercipta** berstatus `CREATED`. Baris itu lalu
memegang seluruh nominal (`pendingAmount` 25.000, `collectibleAmount` 0), sementara:

- tidak ada endpoint untuk merekonsiliasi, membatalkan, maupun menghapus tender menggantung —
  `BillingSettlementsController` hanya punya 4 rute, dan `ReconcileTenderAsync` tidak dipaparkan;
- `POST settlements` kedua ditolak 409 *"Target masih memiliki settlement aktif."*

Akibatnya invoice `BIL-20261001-00000001` (resep `RX-20261001-00003`) **macet permanen** lewat HTTP.
Happy path karena itu dibuktikan pada invoice kedua, dari nol, dengan metode tunai sejak awal.
Ini cacat Billing tersendiri, bukan bagian task ini, dan belum tercatat sebagai gap mana pun.

**Master data `TUNAI` tidak konsisten dengan dirinya sendiri:** `paymentMethodType = "Cash"` tetapi
`isCash = false`. Record itu tidak disentuh sesuai batas scope; dipakai metode uji baru.

## Sisa risiko

- **Producer belum teruji lewat flow konsultasi penuh.** Ia sudah compile dan terpasang di
  `ConsultationFinalizationService`, dan payload ekuivalennya terbukti diterima Billing lewat
  `from-source`. Pengujian lewat konsultasi menjadi regression/integration test terpisah.
- **Amandemen resep**: `PhmPrescription` tidak punya kolom versi, sehingga perubahan item sesudah
  finalisasi akan bentrok 409 pada `SourceVersion` 1.
- **Void policy PHARMACY masih kosong** — pembatalan resep belum dapat membatalkan barisnya.
- **Kategori tarif farmasi** satu-satunya adalah `UJI-TC-PHM` "Farmasi (Uji)" — nama data uji yang
  kini dipakai jalur produksi.
- **`INSURANCE_APPROVED` masih belum pernah terjadi**; tidak ada master metode bayar bertanda
  asuransi atau penjamin.
- **Rincian per obat tidak tampil di invoice** — satu resep satu baris, sesuai bentuk yang dituntut
  penargetan surat clearance.
