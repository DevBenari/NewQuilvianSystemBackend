# Acceptance Test Matrix — Sub-modul `dokter-rawat-inap` (Rawat Inap)

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Sub-modul | `dokter-rawat-inap` |
| Contract version | `0.1.0` |
| `last_changed_in` | `0.1.0` |
| Status | `draft` |
| Tanggal | 2 September 2026 |

Dari **28** skenario di bawah, **12** adalah jalur gagal.

---

## 1. Konteks klinis rawat inap — `INT-DOK-01`, `INT-DOK-02`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `AC-CAP022-01` | Dokter membuat kajian medis pada episode `Admitted` tanpa antrean dan tanpa kunjungan IGD | Integration | `201`; `QueueId` kosong, `InpEpisodeId` terisi |
| `AC-CAP020-02` | SOAP dapat dibuat walaupun **pengkajian awal keperawatan belum selesai** | Integration | `201`; tidak ada pemeriksaan silang ke pengkajian keperawatan |
| `AC-CAP023-01` | Resep dibuat dari konteks rawat inap tanpa kunjungan IGD aktif | Integration | `201` |
| `AC-CAP024-03` | Tindakan tidak menuntut konteks khusus IGD | Integration | `201` |
| `VAL-DOK-04` | **Gagal:** konsultasi rawat jalan tanpa antrean | Integration | `400`; **membuktikan perilaku poliklinik tidak berubah** |
| `RWI-DEC-051` | Konsultasi IGD lewat jalur lamanya tetap berhasil | Regression | `201`; **penjaga regresi wajib** |
| `RWI-RULE-026` aturan 4 | Konsultasi **kedua** pada satu kunjungan rawat inap diterima | Integration | Dua baris konsultasi pada satu encounter |
| `RWI-RULE-026` aturan 4 | **Gagal:** konsultasi kedua pada kunjungan **rawat jalan** tetap ditolak | Regression | Ditolak seperti sebelumnya |
| `RWI-RULE-026` aturan 5 | Resep kedua pada satu konsultasi rawat inap diterima | Integration | Dua resep aktif |

---

## 2. Kajian medis dan SOAP — `CAP-020`, `CAP-022`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `AC-CAP022-02` | Kajian medis dan SOAP punya record serta lifecycle **berbeda** | Integration | Dua tabel berbeda; mengubah SOAP tidak menyentuh baris kajian medis |
| PRD `CAP-022` aturan 3 | SOAP harian **tidak menimpa** kajian medis final | Integration | Isi kajian medis sama persis sebelum dan sesudah tiga SOAP ditulis |
| `AC-CAP020-01` | Dua SOAP pada hari berbeda tersimpan sebagai dua baris lini masa | Integration | Dua baris terurut `ClinicalDateTime` |
| PRD `CAP-020` aturan 2 | Waktu klinis terpisah dari waktu penulisan | Integration | SOAP ditulis pukul 11.00 untuk visite pukul 07.00 tampil pada urutan pukul 07.00 |
| `AC-CAP022-03` | Amandemen kajian medis mempertahankan versi asli | Integration | Status `Amended`; versi lama terbaca utuh |
| `AC-CAP020-03` | **Episode `Closed` menolak SOAP baru** | Integration | `422` |
| `AC-CAP020-03` | Episode `Closed` **menerima amandemen** catatan lama, dan **tidak** mengaktifkan kembali episode | Integration | `200`; `EpisodeStatus` tetap `Closed`; tempat tidur tidak berubah |
| `VAL-DOK-12` | **Gagal:** menyelesaikan SOAP dengan keempat bagian kosong | Integration | `400` |
| `VAL-DOK-11` | **Gagal:** menyelesaikan kajian medis tanpa diagnosis | Integration | `400` |
| `VAL-DOK-14` | **Gagal:** waktu klinis sebelum pasien masuk kamar | Integration | `400` |

---

## 3. CPPT dan verifikasi — `CAP-021`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `AC-CAP021-01` | Catatan dokter dan catatan perawat tampil sebagai **entry terpisah** dengan penulis dan profesi masing-masing | Integration | Dua baris, `ProfessionType` berbeda |
| `AC-CAP021-03` | **Verifikator tidak menjadi penulis asli** | Integration | `ProviderUserId` **tidak berubah**; `VerifiedByUserId` terisi verifikator |
| `AC-CAP021-02` | Keterlambatan verifikasi terpantau menurut kebijakan aktif | Integration | Baris muncul pada daftar pantau |
| `VAL-DOK-24` | Kebijakan verifikasi **belum ada**: seluruh catatan `NotRequired`, daftar pantau kosong | Integration | Pencatatan tetap `201`; nol baris menunggu |
| `VAL-DOK-25` | Catatan terlambat **tidak menahan** penulisan catatan berikutnya | Integration | Catatan berikutnya tetap `201` |
| `VAL-DOK-07` | **Gagal:** dokter jaga yang bukan DPJP mencoba memverifikasi | Integration | `403`; status verifikasi tidak berubah |
| PRD `CAP-021` aturan 6 | Amandemen catatan terverifikasi mengembalikannya ke menunggu verifikasi | Integration | Status kembali `Pending` |

---

## 4. Visite — `CAP-025`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `AC-CAP025-01` | Visite muncul di riwayat **walaupun SOAP baru ditulis beberapa menit kemudian** | Integration | Baris visite ada sebelum SOAP dibuat |
| `AC-CAP025-02` | **SOAP tanpa visite eksplisit tidak menambah hitungan visite** | Integration | Tiga SOAP ditulis; jumlah visite tetap nol |
| `AC-CAP025-03` | Penulis, waktu, dan peran visite dapat diaudit | Integration | `RecordedByUserId`, `VisitDateTime`, `VisitRole` terbaca |
| PRD `CAP-025` aturan 5 | Permintaan berulang dengan kunci idempotency sama menghasilkan **satu** visite | Integration | Panggilan kedua `200` dengan Id yang sama |
| PRD `CAP-025` aturan 5 | Dua permintaan bersamaan dengan kunci sama | Integration terhadap **PostgreSQL sungguhan** | Satu baris. **Provider InMemory tidak dapat membuktikan unique parsial** |
| `VAL-DOK-18` | Visite kedua pada jam berdekatan **diperingatkan, bukan ditolak** | Integration | `200`; dua visite tersimpan bila dilanjutkan |
| `VAL-DOK-08` | **Gagal:** perawat mencoba mencatat visite | Integration | `403` |

---

## 5. Tindakan, resep, dan penunjang — `CAP-023`, `CAP-024`, `CAP-015`

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `AC-CAP024-01` | **Gagal:** tindakan disimpan untuk pasangan pasien dan kunjungan yang tidak cocok | Integration | `400` |
| `AC-CAP024-02` | Percobaan ulang tidak menghasilkan tindakan maupun tagihan ganda | Integration terhadap PostgreSQL | Satu baris tindakan, satu pemicu tagihan |
| PRD `CAP-024` aturan 5 | Kegagalan tagihan **tidak menghilangkan** catatan tindakan | Integration | Catatan `Performed`; pengiriman `Failed` |
| `AC-CAP023-02` | Status pemenuhan resep dapat dibaca kembali dengan pengenal yang sama | Integration | Status Farmasi terbaca |
| `AC-CAP023-03` | **Obat pulang dapat dibedakan** dari resep harian | Integration | `PrescriptionOrderType = Discharge` tersaring tersendiri |
| `VAL-DOK-21` | **Gagal:** sub-modul ini mencoba menandai obat sudah diserahkan | Integration | `403`; status Farmasi tidak berubah |
| `AC-CAP015-01` | **Gagal:** pesanan lab dari episode A diproses sebagai milik episode B | Integration | `400` |
| `AC-CAP015-02` | Hasil lab terverifikasi terbaca dari ruang kerja **tanpa baris salinan** | Integration | Nol tabel hasil baru; data berasal dari modul Laboratorium |
| `VAL-DOK-23` | **Gagal:** sub-modul ini mencoba menulis hasil lab | Integration | `403` |

---

## 6. Yang **belum dapat diuji**

| Butir | Kenapa belum | Kapan dapat diuji |
| --- | --- | --- |
| Pemesanan dan hasil radiologi | **Modulnya tidak ada** | Setelah modul Radiologi berdiri |
| Nilai batas waktu verifikasi CPPT | Menunggu Clinical Governance | Mekanismenya sudah dapat diuji sekarang — bagian 3 |
| Nilai batas waktu kajian medis | `RWI-RULE-021` menunggu pemilik klinis | Sama |
| *Administrative attestation* visite | Kebijakannya belum ada | Setelah kebijakan ditetapkan; bawaan sekarang aman |

---

## 7. Penjaga batas sub-modul

Dua skenario yang tidak diturunkan dari requirement mana pun, melainkan dari `RWI-DEC-081` dan
batas kepemilikan.

| Requirement | Skenario | Jenis test | Bukti yang diharapkan |
| --- | --- | --- | --- |
| `RWI-DEC-081` | Nol tabel berawalan `Inp` yang menyimpan SOAP, CPPT, kajian medis, resep, tindakan, atau visite | Architecture test | Pemindaian `ApplicationDbContext` menemukan **nol** entity demikian |
| `INV-DOK-04`, `INV-DOK-05` | Nol jalur tulis dari modul Rawat Inap menuju status pemenuhan resep maupun hasil laboratorium | Architecture test | Pemindaian endpoint dan service menemukan nol penulisan |

> Test pertama **dibagi** dengan `keperawatan/testing/acceptance-test-matrix.md` bagian 8 — satu
> test yang menjaga kedua sub-modul, bukan dua test kembar.
