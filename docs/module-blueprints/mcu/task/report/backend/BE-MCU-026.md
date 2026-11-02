# Laporan Perubahan Backend — `BE-MCU-026`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-MCU-026` |
| Judul | Perluasan refund Billing |
| Slice | `MVP-2` — `EPIC MCU-04` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian *`BE-MCU-026`* |
| Trace | `MCU-DEC-043`; `MCU-COORD-003`; `MCU-AC-031`; `MCU-EVD-002` 2.2; `GAP-MCU-07`; bagian Billing dari `FR-MCU-034` |
| Contract version | `MCU-API-v1` *Perubahan internal* baris Billing (perilaku `RequestedAmount` berubah, bentuk request tetap); `MCU-INT-v1` `INT-MCU-09` — `approved` 2026-10-21 |
| Dependency | — (persetujuan Yasmina, pemilik `billing-kasir`, ✅ 2026-10-21) |
| Keputusan user (2026-10-22) | (1) Akumulasi per baris di **tabel baru `BilRefundCaseItem`**, migration dibuat tanpa diterapkan; (2) nominal sebagian hanya untuk satu baris, beberapa baris = sisa penuh; (3) perlakuan peserta perusahaan (potong tagihan kolektif) **tidak** diubah di task ini |
| Klasifikasi | `HIGH` — skor 12: berkas diperiksa 9–20 (1), berkas diubah 4–8 (1), logika tinggi (2), entity + migration baru (2), uang — refund (2), modul mitra dipakai kasir (2), idempotensi/konkurensi (1), FE kasir terdampak perilaku (1) |
| Task mode | `BACKEND` (modul Billing, dikerjakan dari roadmap MCU) |
| Target tulis | Billing: `BilRefundCaseItem.cs` (baru), `BilRefundCase.cs`, `BilRefundCaseItemConfiguration.cs` (baru), `BillingRefundService.cs`; `ApplicationDbContext.cs`; migration `20261022081149_AddBillingRefundCaseItem` + snapshot; laporan ini; tanda roadmap/traceability |
| Model | Claude Opus 5.5 |
| Commit backend saat dikerjakan | `e86d98aa` (branch `yoga`) — tidak bergeser |
| Tanggal | 2026-10-22 |
| Status | Selesai — ketiga uji `MCU-EVD-002` 2.2 berbalik; **migration belum diterapkan**; DoD *ditinjau pemilik Billing* menunggu Yasmina |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area / Module | `HealthServices` / `BillingManagement`, prefix `Bil` (registry aktif; checker tidak menandai QBE-MOD-002 atas `BilRefundCaseItem`) |
| Keberlakuan | `NEW CODE` (entity, konfigurasi, migration) + `TOUCHED CODE` (`BillingRefundService`) |
| QBE yang berlaku | `QBE-ENT-001` (entity mewarisi `IdentityModel`), `QBE-CFG-001` (konfigurasi khusus), `QBE-MOD-002` (prefix `Bil`), `QBE-DB-001`/`002` (migration terpisah dari eksekusi), `QBE-SVC-001`, `QBE-TXN-001`, `QBE-VAL-001`, `QBE-LOG-001`, `QBE-CODE-001`..`006` |
| Checker QBE | `PASS` — 6 berkas task (model, konfigurasi, service, DbContext, migration) nol temuan. Kontrol negatif sementara `ZzzKontrol` terkena `QBE-ENT-001`, `QBE-CFG-001`, `QBE-MOD-002`, lalu dihapus |

---

## 1. Masalah yang diperbaiki

`MCU-EVD-002` 2.2 membuktikan tiga cacat jalur refund Billing:
- **Nominal diabaikan.** Permintaan Rp 150.000 atas baris Rp 350.000 tercatat Rp 350.000.
- **Satu case aktif per invoice.** Komponen kedua ditolak selama case pertama belum selesai.
- **Refund ganda.** Sesudah case pertama dieksekusi, baris yang sama dapat direfund lagi, sehingga baris Rp 350.000
  direfund Rp 700.000. Cacat ini berlaku untuk **semua** pemakai refund Billing (`GAP-MCU-07`).

**Contoh sesudah perbaikan.** Baris Paket Basic Rp 350.000 lunas. Thorax dibatalkan → refund Rp 150.000, sisa
Rp 200.000. Audiometri dibatalkan kemudian → refund Rp 50.000, sisa Rp 150.000. Pengajuan Rp 200.000 berikutnya
ditolak: *"Nominal refund Rp 200.000 melebihi sisa yang dapat direfund pada Paket Basic (Rp 150.000)."*

---

## 2. Proses bisnis

1. Pengaju (kasir lewat layar refund, atau MCU lewat batal komponen/batal penuh) memilih baris tagihan dan nominal.
2. Billing menghitung **sisa per baris** = nilai baris − rincian refund dari case yang tidak ditolak (diajukan,
   disetujui, sebagian/seluruhnya dieksekusi).
3. Aturan nominal:
   - nominal kosong/nol → refund utuh atas sisa (perilaku lama);
   - satu baris → nominal sebagian boleh, asal ≤ sisa;
   - beberapa baris → nominal harus sama dengan total sisanya (persis yang dikirim layar kasir hari ini).
4. Case tersimpan beserta **rincian per baris** (`BilRefundCaseItem`) dan baris pengembalian proporsional per tender
   seperti sebelumnya. Persetujuan dan eksekusi tidak berubah.
5. `GET refundable-items` menampilkan sisa per baris; layar kasir otomatis hanya menawarkan sisa itu.

**Jalur tidak normal:**

| Keadaan | Akibat |
| --- | --- |
| Nominal melebihi sisa baris | Validasi (`422` lewat controller Billing yang ada) dengan nominal dan sisa disebut |
| Baris sudah direfund penuh | *"Item … sudah direfund penuh; tidak ada sisa yang dapat direfund."* |
| Beberapa baris dengan nominal sebagian | *"Nominal sebagian hanya dapat diajukan untuk satu item. …"* |
| Case ditolak | Sisa baris kembali |
| Kirim ulang dengan idempotency key sama | Isi sama → case yang sama dikembalikan; nominal berbeda → konflik |
| Refund deposito | Tidak berubah: tetap satu case aktif per invoice |

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

`roadmap/backend-roadmap.md` (`BE-MCU-026`), `evidence/2026-10-21-persetujuan-dan-pembuktian.md` (2.2, 4),
`00-interview-decisions.md` (`MCU-DEC-015`, `043`, `MCU-COORD-003`, `MCU-AC-031`), `integration-contract.md`
(`INT-MCU-09`), `requirement-traceability.md` (`GAP-MCU-07`), `BillingRefundService.cs` (seluruh alur buat, setujui,
eksekusi), `BilRefundCase.cs`, `BilRefundLine.cs` + konfigurasi, `BillingRefundDtos.cs`, FE kasir
`create-refund-modal.jsx` dan `use-billing-financial-exception.js` (cara `requestedAmount` dihitung),
`MigrationMetadata.g.props`, laporan `BE-MCU-003` (konvensi migration).

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `BillingManagement/Billing/Models/BilRefundCaseItem.cs` (baru) | Rincian nominal satu case atas satu baris tagihan |
| `BillingManagement/Billing/Models/BilRefundCase.cs` | Navigasi `Items` |
| `Repositories/Configurations/.../Billing/BilRefundCaseItemConfiguration.cs` (baru) | Tabel `public."BilRefundCaseItem"`, `CHECK Amount > 0`, index unik `(RefundCaseId, InvoiceItemId)`, index `InvoiceItemId`, FK `Restrict` ke `BilRefundCase` dan `BilInvoiceItem` |
| `Repositories/ApplicationDbContext.cs` | `DbSet<BilRefundCaseItem> BilRefundCaseItems` |
| `BillingManagement/Billing/Services/BillingRefundService.cs` | `LoadRemainingRefundableAsync` (sisa per baris; case lama tanpa rincian = refund penuh atas baris di JSON-nya); `CreateAsync` memakai `RequestedAmount` dengan tiga aturan di atas dan menyimpan rincian; satu case aktif per invoice hanya untuk deposito; hash payload memuat nominal; `GetBillingRefundableItemsAsync` menampilkan sisa |
| `Migrations/20261022081149_AddBillingRefundCaseItem*.cs`, `ApplicationDbContextModelSnapshot.cs` | Hanya tabel, dua index, dua FK di atas (§8) |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Bentuk request/response refund tidak berubah. **Perilaku berubah:** `requestedAmount` kini dipakai, dan `refundableAmount` = sisa per baris (sebelumnya 0 atau penuh) |
| Database | Satu tabel baru. Migration **dibuat**, **tidak diterapkan** ke basis data mana pun; penerapan butuh izin terpisah. Sebelum migration diterapkan, aplikasi yang memuat kode ini akan gagal saat membaca/menulis refund BILLING |
| Keamanan/Auth | Tidak berubah — izin endpoint refund Billing yang ada |
| FE kasir | Tanpa perubahan kode: modal mengirim `requestedAmount` = jumlah `refundableAmount` terpilih, yang kini sama dengan sisa. Baris yang sudah direfund sebagian kini **tampil** dengan sisanya (sebelumnya disembunyikan sebagai 0) |

### 3.4 Delta terhadap roadmap

| Butir | Roadmap | Implementasi | Alasan |
| --- | --- | --- | --- |
| Bentuk akumulasi | *Ditetapkan pemilik Billing saat eksekusi* | Tabel `BilRefundCaseItem` | Dipilih user 2026-10-22; akumulasi tidak dapat diturunkan andal dari `SelectedBillingItemIdsJson` (tanpa nominal per baris) |
| Nominal pada beberapa baris | Tidak dirinci | Harus sama dengan total sisa | Dipilih user 2026-10-22; menjaga rincian per baris tetap tegas dan sama dengan layar kasir hari ini |
| Satu case aktif per invoice | Tidak disebut | Dilepas untuk BILLING, tetap untuk DEPOSITO | Dibutuhkan *"komponen kedua diterima sesudah case pertama"*; refund ganda kini dijaga akumulasi per baris |
| Peserta perusahaan | Ditetapkan pemilik Billing | Tidak diubah | Dipilih user 2026-10-22 |
| Uji permanen di `Tests/` | Uji harness `MCU-EVD-002` dijadikan permanen | Uji dijalankan di folder sementara lalu **dihapus** | Instruksi user: berkas uji MCU dihapus sesudah backend teruji. Bila uji permanen tetap diinginkan, beri tahu — tidak ada proyek uji Billing yang dapat dikompilasi saat ini (`PharmacyTests` gagal kompilasi sejak sebelum task ini) |
| Ditinjau pemilik Billing | DoD | Belum | Menunggu Yasmina |

---

## 4. Dokumentasi endpoint

Tidak ada endpoint baru. Perilaku yang berubah pada endpoint Billing yang ada:

| Endpoint | Perubahan |
| --- | --- |
| Buat refund (`BillingRefundService.CreateAsync`) | `requestedAmount` dipakai (≤ sisa baris); beberapa case BILLING boleh aktif bersamaan pada satu invoice |
| Item refundable (`GetBillingRefundableItemsAsync`) | `refundableAmount` = sisa per baris |

---

## 5. Verifikasi

Harness uji **sementara** (`Tests/QuilvianSystemBackend.McuTests`, SQLite, `BillingRefundService` produksi; invoice
satu/dua baris, lunas tunai dengan metode pembayaran yang dapat direfund) dijalankan lalu **dihapus** sesuai
instruksi user.

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build -p:RunAnalyzers=False` (dua kali, termasuk target validasi riwayat migration) | 0 error | `PASS` | Keluaran build |
| `dotnet ef migrations add AddBillingRefundCaseItem --no-build` | Isi hanya `BilRefundCaseItem` | `PASS` | §8 |
| `dotnet ef migrations has-pending-model-changes --no-build` | *"No changes have been made to the model since the last migration."* | `PASS` | Keluaran perintah |
| `dotnet test Tests/QuilvianSystemBackend.McuTests` | 11 lulus | `PASS` | `Passed! - Failed: 0, Passed: 11` |
| `MCU-EVD-002` 2.2 uji 1 | Permintaan Rp 150.000 → case Rp 150.000, satu baris pengembalian Rp 150.000, rincian baris paket Rp 150.000 | `PASS` | `Uji_1_…` |
| Uji 2 | Rp 50.000 kedua diterima **selagi case pertama masih diajukan**; sisa baris Rp 150.000 | `PASS` | `Uji_2_…` |
| Uji 3 | Rp 200.000 sesudah 150.000 + 50.000 ditolak dengan pesan persis; Rp 150.000 diterima; sesudahnya Rp 1 ditolak *sudah direfund penuh*; total rincian tepat Rp 350.000 | `PASS` | `Uji_3_…` |
| Celah refund ganda | Case pertama `EXECUTED` → refund kedua Rp 350.000 ditolak; sisa 0 | `PASS` | `Celah_lama_tertutup_…` |
| Case ditolak | Sisa kembali Rp 350.000 dan dapat diajukan ulang | `PASS` | `Case_ditolak_…` |
| Regresi kasir | Dua baris (350.000 + 75.000) dengan nominal = total sisa → case Rp 425.000, rincian per baris penuh, sisa 0; satu baris tanpa nominal → Rp 350.000 (perilaku lama) | `PASS` | `Regresi_kasir_…`, `Regresi_refund_utuh_…` |
| Beberapa baris nominal sebagian | Ditolak dengan pesan persis; nol case | `PASS` | `Beberapa_item_…` |
| Case lama tanpa rincian | Dianggap refund penuh baris di JSON-nya; baris lain tetap utuh | `PASS` | `Case_lama_…` |
| Idempotensi | Kirim ulang sama → case sama; nominal beda dengan key sama → konflik; tetap 1 case | `PASS` | `Idempotensi_…` |
| Batas nominal | Melebihi nilai baris ditolak; Rp 0,01 diterima | `PASS` | `Nominal_melebihi_…` |
| Uji mutasi | Nominal satu baris dikembalikan ke *nilai baris penuh* dan pemeriksaan *sudah penuh* dilepas → 4 uji **gagal**; dipulihkan identik (`cmp`), kembali 11/11 | `PASS` | Keluaran `dotnet test` |
| Checker QBE `-Path` (6 berkas + kontrol) | Nol temuan; kontrol terkena | `PASS` | Keluaran checker |
| `dotnet ef migrations script AddCoveringMcuOrderToLabRadOrders AddBillingRefundCaseItem --no-build` (offline) | SQL terlampir §8 | `PASS` | Lampiran |
| Refund deposito; persetujuan dan eksekusi lewat penyedia pembayaran; PostgreSQL; layar kasir di browser | Tidak dijalankan — jalurnya tidak diubah selain penjaga case aktif | `NOT RUN` | — |
| Regresi batal penuh `BE-MCU-025` | Memanggil `CreateAsync` dengan nominal = nilai baris (satu baris) — jalur yang sama dengan `Case_ditolak_…`/`Regresi_refund_utuh_…`; harness `BE-MCU-025` tidak dijalankan ulang | `PARTIAL` | — |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| (1) Permintaan Rp 150.000 tercatat Rp 150.000 | Terpenuhi | §5 |
| (2) Komponen kedua Rp 50.000 pada baris yang sama diterima sesudah case pertama | Terpenuhi | §5 |
| (3) Total atas baris Rp 350.000 tidak pernah melebihi Rp 350.000; Rp 200.000 sesudah 150.000 + 50.000 ditolak | Terpenuhi | §5 |
| Refund utuh tanpa `RequestedAmount` berperilaku sama (regresi kasir) | Terpenuhi | §5 |
| DoD: laporan task ditinjau pemilik Billing | **Belum** — menunggu Yasmina | — |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Migration wajib diterapkan bersamaan dengan kode ini.** Tanpa tabel `BilRefundCaseItem`, membuat refund BILLING dan membuka daftar item refundable akan gagal. Di devYoga (`migrations list` 2026-10-23, baca-saja) tertunda tepat dua: `20261022021727_AddCoveringMcuOrderToLabRadOrders` dan `20261022081149_AddBillingRefundCaseItem`; `AddMedicalCheckupManagement` sudah diterapkan. Keduanya berurutan di ujung, jadi skrip `AddMedicalCheckupManagement AddBillingRefundCaseItem` cukup |
| Masalah yang diketahui | Perlakuan peserta perusahaan (potong tagihan kolektif) belum ada; refund hanya atas tender yang benar-benar dibayar. Bila tidak ada tender sama sekali, jalur lama tetap membuat baris pengembalian *tunai* fiktif (perilaku yang sudah ada, tidak disentuh) |
| Risiko tersisa | Perilaku layar kasir berubah: baris yang direfund sebagian kini tampil dengan sisanya, dan beberapa case dapat aktif bersamaan pada satu invoice. Isolasi Serializable dan kunci penasihat hanya teruji di SQLite. Persetujuan tertulis Yasmina atas bentuk teknis belum tercatat |
| Perubahan sampingan | Folder uji sementara dibuat dan dihapus; kontrol negatif QBE dibuat dan dihapus; berkas baru disimpan CRLF mengikuti berkas Billing sejenis |
| Interupsi | Tidak ada |
| Status Git | ` M` `BilRefundCase.cs`, `BillingRefundService.cs`, `ApplicationDbContext.cs`, `ApplicationDbContextModelSnapshot.cs`; `??` `BilRefundCaseItem.cs`, `BilRefundCaseItemConfiguration.cs`, dua berkas `20261022081149_AddBillingRefundCaseItem*`; laporan ini; tanda roadmap/traceability |
| Langkah berikutnya | `BE-MCU-024` (batal komponen + refund) kini siap — dependency `BE-MCU-021` ✅ dan `BE-MCU-026` ✅ |

---

## 8. Lampiran — SQL migration (offline, belum diterapkan)

Dihasilkan `dotnet ef migrations script AddCoveringMcuOrderToLabRadOrders AddBillingRefundCaseItem --no-build`.

```sql
START TRANSACTION;
CREATE TABLE public."BilRefundCaseItem" (
    "Id" uuid NOT NULL,
    "RefundCaseId" uuid NOT NULL,
    "InvoiceItemId" uuid NOT NULL,
    "Amount" numeric(18,2) NOT NULL,
    "CreateDateTime" timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
    "CreateBy" uuid NOT NULL,
    "UpdateDateTime" timestamp with time zone,
    "UpdateBy" uuid NOT NULL,
    "DeleteDateTime" timestamp with time zone,
    "DeleteBy" uuid NOT NULL,
    "CancelDateTime" timestamp with time zone,
    "CancelBy" uuid NOT NULL,
    "IsCancel" boolean NOT NULL DEFAULT FALSE,
    "IsDelete" boolean NOT NULL DEFAULT FALSE,
    CONSTRAINT "PK_BilRefundCaseItem" PRIMARY KEY ("Id"),
    CONSTRAINT "CK_BilRefundCaseItem_Amount" CHECK ("Amount" > 0),
    CONSTRAINT "FK_BilRefundCaseItem_BilInvoiceItem_InvoiceItemId" FOREIGN KEY ("InvoiceItemId") REFERENCES public."BilInvoiceItem" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_BilRefundCaseItem_BilRefundCase_RefundCaseId" FOREIGN KEY ("RefundCaseId") REFERENCES public."BilRefundCase" ("Id") ON DELETE RESTRICT
);
CREATE INDEX "IX_BilRefundCaseItem_InvoiceItemId" ON public."BilRefundCaseItem" ("InvoiceItemId");
CREATE UNIQUE INDEX "IX_BilRefundCaseItem_RefundCaseId_InvoiceItemId" ON public."BilRefundCaseItem" ("RefundCaseId", "InvoiceItemId");
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261022081149_AddBillingRefundCaseItem', '9.0.18');
COMMIT;
```
