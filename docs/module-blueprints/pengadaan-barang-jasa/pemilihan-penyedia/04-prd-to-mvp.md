# Pemilihan Penyedia — PRD ke MVP

## 1. Identitas dokumen

| Field | Nilai |
| --- | --- |
| Produk / modul | Quilvian · Pengadaan Barang & Jasa (`PBJ-BP-001`) · sub-modul `pemilihan-penyedia` (`PBJ-PS`) |
| Kontrak | `PBJ-PS-MVP` · `last_changed_in: 1.0` · status `draft` |
| Repository target | `NewQuilvianSystemBackend` @`d5fdc8fa`, `V2QuilvianSystemFrontendDev` @`3e5386679` |
| Cakupan | K-05 Persiapan Pengadaan dan K-07 Penawaran & Pemilihan Penyedia: dari baris PR yang disetujui sampai nomor PO BLM tercatat pada paket |

## 2. Ringkasan eksekutif

Hari ini PR yang disetujui berhenti di status "Disetujui", lalu pemilihan vendor dikerjakan di luar sistem tanpa jejak.
Sub-modul ini membuat pemilihan vendor dapat diaudit: setiap paket punya HPS dan metode yang disetujui, vendor yang diundang
pasti layak, penawaran tercatat dengan waktu diterimanya, evaluasi teknis dilakukan orang yang tidak mencatat harga, dan
penetapan vendor disetujui berjenjang lalu langsung tercatat sebagai komitmen pagu per unit.

## 3. Masalah produk

| Ada (bukti) | Belum ada |
| --- | --- |
| PR `APPROVED` dan baris beli (`PrcPurchaseRequestLine.PurchaseQuantity`) | Paket eksekusi, RFQ, penawaran, evaluasi, penetapan (`PBJ-CAP-019`..`030`) |
| Kelayakan vendor per kategori (`ProcurementVendorEligibilityService`) | Cara memakainya untuk RFQ multi-kategori |
| Pencatat/pelepas komitmen pagu (`ProcurementBudgetCommitmentService`), belum ada pemanggil | Pemanggil per garis pagu |
| Mesin `Wfl` dengan langkah bersyarat dan pengajuan atas nama pemohon | Definisi persiapan dan penetapan |
| Lampiran aman per dokumen | Lampiran harga tertutup bagi evaluator (`PBJ-CAP-004`) |

## 4. Visi produk

Baris PR disetujui → paket (gabungan per barang, asal tiap unit tetap tercatat) → HPS dan metode disetujui → RFQ ke vendor
layak → penawaran tercatat (termasuk PPN) → evaluasi teknis tanpa harga → negosiasi → penetapan satu vendor disetujui →
komitmen pagu per unit → nomor PO BLM → diteruskan ke `kontrak-pelaksanaan`.

## 5. Batas MVP

**Titik mulai:**
1. Ada PR berstatus `APPROVED` dengan jumlah beli > 0.
2. Metode, jenis dokumen, dan dokumen wajib sudah diisi (minimal nilai contoh).
3. Definisi `PBJ-PKT-PERSIAPAN` dan `PBJ-PKT-PENETAPAN` aktif dengan matriks.

**Titik akhir:**
1. Paket berstatus `ORDERED` dengan nomor PO BLM unik.
2. Komitmen pagu aktif per garis pagu berjumlah sama dengan nilai penetapan.
3. Riwayat paket, RFQ, penawaran, evaluasi, dan penetapan lengkap.

## 6. Pelaku sasaran

| Pelaku | Tanggung jawab dalam MVP |
| --- | --- |
| Tim Pengadaan | Membuat paket, persiapan, RFQ, mencatat penawaran dan negosiasi, mengajukan penetapan, mencatat nomor PO |
| Manajer Pengadaan | Seperti Tim Pengadaan + mencabut penetapan; penyetuju bila matriks menunjuk |
| Evaluator teknis | Menilai Lulus/Tidak lulus per kriteria pada paket yang ditugaskan |
| Penyetuju (sesuai matriks SOP) | Memutus persiapan dan penetapan |
| Keuangan | Membaca nilai penetapan dan komitmen |
| Unit peminta | Melihat nomor, status, dan vendor terpilih untuk PR-nya |

## 7. Pemilihan kemampuan MVP

| Kemampuan | ID asal | Kenapa wajib |
| --- | --- | --- |
| Paket dari baris PR | `PBJ-CAP-019`, `020` | Tanpa paket tidak ada pemilihan vendor |
| Persiapan: HPS, metode, checklist | `PBJ-CAP-027`, `028` | Dasar persetujuan dan batas penetapan |
| Persetujuan persiapan dan penetapan | `PBJ-CAP-001`, `024` | `PBJ-DEC-072` |
| RFQ ke vendor layak | `PBJ-CAP-023` | `PBJ-DEC-045`, `054` |
| Pencatatan penawaran dan PPN | `PBJ-CAP-022` (makna nilai) | `PBJ-DEC-067`, `084` |
| Evaluasi teknis tanpa harga | `PBJ-CAP-025`, `026`, `004` | `PBJ-DEC-074`, `079` |
| Negosiasi dan perbandingan | — (baru) | `PBJ-DEC-073`, `085` |
| Penetapan + komitmen pagu | `PBJ-CAP-022`, `023` | `PBJ-DEC-076`, `078` |
| Nomor PO, batal, pecah, gagal | `PBJ-CAP-014`, `020` | `PBJ-DEC-011`, `075`, `077`, `083` |

## 8. Kemampuan yang ditunda

| Kemampuan | Alasan | Pengganti selama MVP |
| --- | --- | --- |
| Wajib evaluasi teknis per kategori (`G-P10`) | Usulan; aturan per metode sudah cukup untuk rilis pertama | Pilih metode yang mewajibkan evaluasi teknis untuk alkes |
| Laporan "direncanakan vs dipaketkan" | Laporan modul `POST-MVP` (`02-module-map.md`) | Daftar paket bersaring + kolom rencana paket rujukan |
| Pencocokan nomor PO ke PO Finance | Finance belum punya filter nomor PO; BLM belum terintegrasi (`PBJ-CAP-014`, `FIN-OQ-141`) | Nomor PO sebagai teks unik |
| Notifikasi aktif (email) | `PBJ-OQ-015` | Notifikasi tugas `Wfl` + tanda di daftar (`G-P07`) |
| Portal vendor | `PBJ-DEC-067` (tidak dibuat) | RFQ diunduh dan dikirim lewat email |

## 9. Alur bisnis target

`FLOW-PS-MVP-001`: (1) pilih baris PR → (2) susun persiapan → (3) ajukan dan setujui persiapan → (4) buat dan kirim RFQ →
(5) catat penawaran → (6) tutup RFQ → (7) evaluasi teknis → (8) negosiasi → (9) ajukan penetapan → (10) setujui dan terapkan
(komitmen) → (11) catat nomor PO. Rincian di `flowcharts/`.

## 10. Epic dan functional requirement

### `EPIC PS-01` — Konfigurasi pemilihan · disposisi `MISSING / NEW`
- `FR-PS-001` Admin mengelola metode beserta rentang HPS, minimum penawaran, wajib evaluasi teknis (`PBJ-DEC-068`).
- `FR-PS-002` Admin mengelola jenis dokumen paket (tahap, sensitif harga) dan dokumen wajib per metode atau jenis pengadaan (`PBJ-ASM-007`).
- `FR-PS-003` Seeder mengisi nilai contoh bertanda `IsSample` (`PBJ-DEC-069`).

### `EPIC PS-02` — Paket dan persiapan · disposisi `MISSING / NEW` + `EXTEND` (`perencanaan-permintaan`)
- `FR-PS-010` Paket dibuat dari baris PR `APPROVED`; satu baris di satu paket hidup (`PBJ-AC-026`).
- `FR-PS-011` PR `CONVERTED_TO_PACKAGE` setelah semua baris beli berpaket (`PBJ-AC-027`).
- `FR-PS-012` HPS wajib; metode disarankan dari HPS dan disalin; di luar rentang wajib alasan (`PBJ-AC-022`, `023`).
- `FR-PS-013` Checklist wajib lengkap sebelum diajukan.
- `FR-PS-014` Persiapan disetujui lewat `Wfl`; HPS terkunci setelah disetujui.
- `FR-PS-015` Pecah paket sampai sebelum penetapan; metode asal tetap (`PBJ-AC-034`).
- `FR-PS-016` Batal sampai sebelum nomor PO; baris PR dan komitmen dilepas (`PBJ-AC-033`).

### `EPIC PS-03` — RFQ dan penawaran · disposisi `MISSING / NEW`
- `FR-PS-020` RFQ hanya setelah persiapan disetujui (`PBJ-AC-024`), hanya ke vendor layak untuk semua kategori; Dibatasi wajib alasan.
- `FR-PS-021` Batas waktu mengikat; perpanjangan sebelum lewat, berlaku untuk semua (`PBJ-AC-030`).
- `FR-PS-022` Penawaran dicatat Tim Pengadaan dengan waktu diterima dan PPN; harga akhir termasuk PPN (`PBJ-AC-035`).
- `FR-PS-023` RFQ gagal → RFQ ulang atau ganti metode (`PBJ-DEC-075`).

### `EPIC PS-04` — Evaluasi teknis · disposisi `MISSING / NEW` + `EXTEND` (lampiran)
- `FR-PS-030` Evaluator ditunjuk per paket; pencatat tidak mengevaluasi (`PBJ-AC-028`).
- `FR-PS-031` Evaluator tidak menerima harga maupun lampiran harga (`PBJ-AC-029`).
- `FR-PS-032` Hasil Lulus/Tidak lulus per kriteria; Tidak lulus wajib alasan.

### `EPIC PS-05` — Perbandingan, negosiasi, penetapan · disposisi `MISSING / NEW` + `REUSE` (komitmen, kelayakan)
- `FR-PS-040` Perbandingan harga akhir dan peringkat; terendah ditandai (`PBJ-DEC-085`).
- `FR-PS-041` Negosiasi per putaran; harga tidak naik.
- `FR-PS-042` Penetapan satu vendor: ≤ HPS atau alasan; terendah atau alasan; vendor layak (`PBJ-AC-025`, `031`, `036`).
- `FR-PS-043` Penetapan disetujui → komitmen per garis pagu berjumlah nilai penetapan (`PBJ-AC-032`); vendor tidak layak saat diterapkan → tidak diterapkan (`G-P08`).
- `FR-PS-044` Cabut penetapan sebelum nomor PO melepas komitmen (`G-P09`).

### `EPIC PS-06` — Nomor PO dan serah ke kontrak · disposisi `MISSING / NEW`
- `FR-PS-050` Nomor PO dicatat setelah penetapan disetujui; unik antar paket (`PBJ-AC-006`).
- `FR-PS-051` Unit peminta melihat nomor, status, vendor terpilih paket dari PR-nya tanpa harga.

## 11. Model status yang diusulkan

Paket: `DRAFT` → `PREPARATION_PENDING_APPROVAL` → `READY_FOR_RFQ` → `RFQ_OPEN` → `EVALUATION` → `AWARD_PENDING_APPROVAL` →
`AWARDED` → `ORDERED`; cabang `PREPARATION_RETURNED`, `REJECTED`, `CANCELLED`. RFQ: `DRAFT`/`SENT`/`CLOSED`/`FAILED`/`CANCELLED`.
Penetapan: `PENDING_APPROVAL`/`RETURNED`/`APPROVED`/`REJECTED`/`WITHDRAWN`/`REVOKED`/`NOT_APPLIED`. Invariant utama di
`contracts/state-transition-matrix.md`.

## 12. Sasaran arsitektur

Dipakai ulang: `Wfl` + gateway, nomor, riwayat, cakupan unit, kelayakan vendor, komitmen pagu, komponen FE bersama.
Diperluas: baris PR (`PackageId`, tiga metode integrasi), lampiran (saringan harga), konstanta Common. Baru: submodule
`Sourcing` (14 tabel), tiga master, lima controller. Rincian di `02-backend-architecture.md`.

## 13. Sasaran kemampuan API

Seluruh endpoint di `contracts/api-contract.md` grup `Sourcing / Procurement Package`, `Procurement RFQ`, `Procurement
Quotation`, `Technical Evaluation`, `Package Award`, dan tiga master — semuanya **Rencana (belum tersedia)**. Bukti per FR:
`FR-PS-010` → `POST /procurement-packages`; `FR-PS-022` → `POST /procurement-quotations`; `FR-PS-031` → `GET
/technical-evaluations/{packageId}`; `FR-PS-042` → `POST /procurement-package-awards`; `FR-PS-050` → `POST
/procurement-packages/{id}/purchase-order`.

## 14. Matriks kewenangan

Lihat `contracts/permission-audit-matrix.md` bagian 3 (bundel `PBJ-DEC-080`). Kunci: `ProcurementPackage : ReadPrice`
hanya untuk Tim Pengadaan dan penyetuju; evaluator hanya `ProcurementTechnicalEvaluation : Read, Evaluate`.

## 15. Batas integrasi dan billing

Sub-modul ini **tidak** membuat PO, GR, faktur, utang, maupun tagihan pasien. Ia **tidak** menulis tabel
`perencanaan-permintaan` secara langsung (lewat `MarkLinesPackagedAsync`, `ReleaseLinesAsync`, `CommitAsync`,
`ReleaseAsync`), dan **tidak** mengubah `MstSupplier`.

## 16. Guardrail regulasi

RS swasta mengikuti SOP internal (`PBJ-DEC-006`); tidak ada kewajiban rekam medis. Kewajiban yang mengikat MVP: jejak audit
pemilihan vendor (siapa mencatat, mengevaluasi, menyetujui, kapan, alasan) dan kerahasiaan harga sebelum penetapan
(`PBJ-DEC-079`). Pajak hanya informasi per penawaran (`PBJ-DEC-084`).

## 17. Kebutuhan non-fungsional

| ID | Kebutuhan |
| --- | --- |
| `NFR-001` | Aksi status atomik bersama riwayat dan panggilan service pemilik lain (`02-backend-architecture.md` bagian 1) |
| `NFR-002` | `RowVersion` pada paket, RFQ, penawaran, penetapan; 409 `PBJ-VAL-090` |
| `NFR-003` | Index unik: baris PR hidup, penetapan hidup per paket, nomor paket/RFQ/PO |
| `NFR-004` | `Idempotency-Key` pada aksi status; pengajuan `Wfl` idempoten |
| `NFR-005` | Waktu disimpan `timestamptz`; batas waktu RFQ dibandingkan dalam zona waktu operasional rumah sakit |
| `NFR-006` | Harga tidak masuk log; harga tertutup di API, bukan hanya di layar |

## 18. Skenario UAT

| ID | Epic | Jalur | Skenario | Hasil yang diharapkan |
| --- | --- | --- | --- | --- |
| `UAT-PS-01` | PS-01 | Berhasil | Admin mengganti metode contoh dengan metode SOP | Metode baru aktif; metode contoh nonaktif |
| `UAT-PS-02` | PS-01 | Gagal | Admin menghapus metode yang dipakai paket | Ditolak "sudah dipakai, nonaktifkan saja" |
| `UAT-PS-03` | PS-02 | Berhasil | Tim Pengadaan membuat paket kursi roda dari dua PR dan mengajukan persiapan | Paket menunggu persetujuan; penyetuju menyetujui → siap RFQ |
| `UAT-PS-04` | PS-02 | Gagal | Mengajukan paket jasa tanpa KAK/TOR | Ditolak dengan daftar dokumen kurang |
| `UAT-PS-05` | PS-03 | Berhasil | Mengirim RFQ ke tiga vendor layak, mencatat tiga penawaran tepat waktu | Tiga penawaran sah |
| `UAT-PS-06` | PS-03 | Gagal | Mencatat penawaran yang datang setelah batas waktu | Tercatat "Terlambat — tidak sah" |
| `UAT-PS-07` | PS-04 | Berhasil | Teknisi IPSRS menilai tiga penawaran tanpa melihat harga | Hasil tersimpan; tidak ada angka harga di layar |
| `UAT-PS-08` | PS-04 | Gagal | Pencatat mencoba menilai penawaran yang ia catat | Tombol tidak ada; dipaksa lewat API → ditolak |
| `UAT-PS-09` | PS-05 | Berhasil | Negosiasi menurunkan harga di bawah HPS, penetapan disetujui | Paket ditetapkan; komitmen pagu IGD dan Rawat Inap bertambah |
| `UAT-PS-10` | PS-05 | Gagal | Memilih vendor bukan terendah tanpa alasan | Ditolak, diminta alasan |
| `UAT-PS-11` | PS-06 | Berhasil | Mencatat nomor PO dari BLM | Paket "Sudah dipesan"; unit peminta melihat nomor paket dan vendor |
| `UAT-PS-12` | PS-06 | Gagal | Mencatat nomor PO yang sudah dipakai paket lain | Ditolak dengan nomor paket pemakainya |
| `UAT-PS-13` | PS-02 | Berhasil | Memecah tongkat ke paket baru setelah penawaran | Paket baru membawa penawaran tongkat dan metode yang sama |
| `UAT-PS-14` | PS-02 | Berhasil | Membatalkan paket yang sudah ditetapkan karena vendor mundur | Komitmen dilepas; PR kembali dapat dipaketkan |

## 19. Definition of Done

| Butir | Jawaban |
| --- | --- |
| Registry submodule `Sourcing` `ACTIVE` sebelum model pertama | ya / belum (`AT-PS-60`) |
| Satu paket berjalan dari baris PR sampai `ORDERED` di dev | ya / belum (`AT-PS-01`, `15`, `44`, `50`) |
| Evaluator tidak menerima harga dari API mana pun | ya / belum (`AT-PS-31`) |
| Pencatat tidak dapat mengevaluasi; pengaju tidak dapat menyetujui | ya / belum (`AT-PS-30`, `16`) |
| Jumlah komitmen = nilai penetapan per paket | ya / belum (`AT-PS-44`) |
| Batal dan cabut melepas komitmen dan baris PR | ya / belum (`AT-PS-52`, `55`) |
| Pecah paket tidak menurunkan metode | ya / belum (`AT-PS-53`) |
| Seeder idempoten; master terisi | ya / belum (`AT-PS-64`) |
| Setiap layar terjangkau dari menu atau layar induk | ya / belum (`AT-PS-63`; menunggu `PBJ-OQ-038`) |
| Build backend dan frontend sukses; skema sesuai kamus data | ya / belum (`AT-PS-61`, `62`) |
| Regresi `perencanaan-permintaan` (PR, verifikasi, pagu) lulus setelah perubahan baris PR dan lampiran | ya / belum |

## 20. Urutan pengiriman dan pertanyaan terbuka

| Gelombang | Epic | Syarat |
| --- | --- | --- |
| `MVP-PS0` | Registry `Sourcing`; `PS-01`; perluasan baris PR dan lampiran (`perencanaan-permintaan`) | Approval desain ini + baris registry |
| `MVP-PS1` | `PS-02` | `MVP-PS0` |
| `MVP-PS2` | `PS-03`, `PS-04` | `MVP-PS1` |
| `MVP-PS3` | `PS-05`, `PS-06` | `MVP-PS2` |
| `POST-MVP` | Kemampuan bagian 8 | — |

| Pertanyaan | Pemilik | Memblokir |
| --- | --- | --- |
| `PBJ-OQ-038` nama 11 submenu menu 4 dan 6 | Pengguna sesi | Desain frontend saja (label menu); **tidak** memblokir `MVP-PS0`..`PS3` backend |
| `PBJ-OQ-005` metode dan batas nilai SOP | Pengguna sesi | Pemakaian nyata, bukan development |
| `PBJ-OQ-019` penyetuju persiapan dan penetapan | Pengguna sesi + admin `Wfl` | Pemakaian nyata |
| Usulan `G-P08`, `G-P09`, `G-P15` (tidak diterapkan, cabut, garis pagu wajib ada) | Pengguna sesi | Dikonfirmasi saat approval desain ini |
| Usulan `G-P02`, `G-P05`..`G-P07`, `G-P11`, `G-P12` | Pengguna sesi | Tidak memblokir; tampil sebagai usulan |
