# Laporan Perubahan Backend — `BE-LAB-24`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-LAB-24` |
| Judul | Penguncian daftar pemeriksaan pada jalur hapus |
| Slice | `EPIC-LAB-11`, gelombang `MVP-5a` |
| Roadmap | [`roadmap/backend-roadmap.md`](../../../roadmap/backend-roadmap.md) bagian 6b |
| Trace | `FR-11.7`; `LAB-DEC-039`, BR-34; `AC-55`, `AC-56`, `AC-57` |
| Contract version | `LAB-VAL-v1` `r3` `VAL-18` — **tidak berubah** |
| Dependency | — |
| Klasifikasi | `LIGHT` — pemeriksaan, bukan pembangunan |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — source Laboratorium |
| Model | Claude Opus 5 |
| Commit backend saat dikerjakan | `9067fa73`, branch `yoga` |
| Tanggal | 2026-09-14 |
| Status | **✅ `SELESAI`** — 2026-09-14. **Nol baris source diubah, dan memang tidak perlu.** Keempat acceptance criteria terpenuhi oleh kode yang sudah berjalan; lihat bagian 8 |

> ## Addendum — ditutup 2026-09-14
>
> Pemilik modul memilih **pilihan A**: `AC-55` dan `AC-57` **dipersempit ke penambahan saja**
> lewat `LAB-DEC-049`. Pembatalan tetap terbuka sesudah kelayakan, dan koreksi tagihannya
> wewenang Billing sesuai `LAB-INH-010`.
>
> **`RJ-BIL-GATE-DEC-003` tidak perlu disentuh**, dan tidak ada persetujuan di luar modul yang
> diperlukan.
>
> **Cakupan `BE-LAB-24` yang tersisa:**
>
> | Butir | Keadaan |
> |---|---|
> | Jalur tambah terjaga `VAL-18` | **Sudah berjalan** — tidak ada yang perlu dibangun |
> | Jalur batal tidak dikunci | **Memang seharusnya begitu** — `LAB-DEC-049` |
> | `AC-56` | **Terbukti** pada pemeriksaan ini |
> | Penjaga `T-55b`, `T-55c` | **Belum ditulis** — menjaga perilaku yang sudah benar |
> | Penjaga `T-55e` | **Belum ditulis** — menjaga agar pembatalan tidak diam-diam ditutup seseorang kelak |
>
> `T-55d` dicabut dan digantikan `T-55e` yang arahnya berlawanan: ia menguji bahwa pembatalan
> **tetap berhasil**. Penjaga terhadap seseorang yang kelak menambahkan `VAL-18` ke jalur batal
> dengan maksud baik, lalu menutup jalur sah menurut `LAB-INH-006` tanpa ada yang menyadarinya.
>
> Layar juga memperoleh kewajiban baru dari `BR-44`: saat tombol batal ditekan sesudah wadah
> dinyatakan `Layak`, layar **wajib mengatakan** bahwa tagihannya baru gugur setelah Billing
> mengoreksi — bukan membiarkannya menjadi kejutan di loket.

## Backend Governance Preflight

| Field | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `LaboratoryManagement / Laboratory` |
| Pemilik dan prefix registry | Prefix `Lab`, lifecycle `ACTIVE` |
| Keberlakuan | Tidak ada perubahan source. Bila kelak dilanjutkan: `TOUCHED LEGACY` pada `LabExaminationService` |
| QBE ID yang berlaku | `QBE-MOD-001` — sisanya tidak berlaku karena tidak ada source yang disentuh |

---

## 1. Yang diperiksa

Task ini berdisposisi `EXISTING / REUSE`. Yang diminta bukan membangun, melainkan memastikan
**jalur hapus** baris pemeriksaan memakai penjagaan `VAL-18` yang sama dengan jalur tambah.

### 1.1 Jalur tambah — terbukti sudah terjaga

`Areas/HealthServices/LaboratoryManagement/Services/LabExaminationService.cs:120-127`:

```csharp
// VAL-18. Wadah yang sudah diputuskan tidak boleh bertambah isinya: kelayakan
// tagihnya sudah terbit, dan menambah pemeriksaan sesudahnya berarti menagihkan
// sesuatu yang tidak pernah ikut dinilai layak.
if (specimen.SpecimenStatus is LabSpecimenStatus.Accepted or LabSpecimenStatus.Rejected)
```

Sesuai dugaan roadmap. `LAB-DEC-039` memang menyamakan catatan dengan kenyataan yang sudah
berjalan, bukan mengubah perilaku.

### 1.2 Jalur hapus — ternyata tidak ada `DELETE` sama sekali

Grup Lab Examination tidak punya endpoint `DELETE`. Yang ada `POST /{id}/cancel`, mengikuti
`rules/backend/transaction-endpoint-standard.md`: transaksi dibatalkan lewat aksi `cancel`,
bukan dihapus.

`CancelAsync` menjaga dua hal:

| Penjaga | Kondisi |
| --- | --- |
| `VAL-19` | Pemeriksaan sudah `Voided` — gugur bersama wadah yang ditolak |
| — | Pemeriksaan sudah `Cancelled` |

**Tidak ada penjagaan atas status wadah.** Pemeriksaan pada wadah yang sudah `Accepted` **dapat
dibatalkan**.

---

## 2. Kenapa ini bukan celah yang tinggal ditambal

Dugaan awal roadmap: jalur hapus lupa diberi penjagaan yang sama. Pemeriksaan menunjukkan
sebaliknya — **ketiadaannya disengaja**, dan tertulis di kode itu sendiri:

> *"Pembatalan pemeriksaan yang sudah layak tagih punya akibat finansial, dan Billing yang
> menentukan koreksinya. Yang dikerjakan di sini hanya mencatat sebabnya selengkap mungkin;
> penerbitan fakta pembatalannya adalah pekerjaan `BE-LAB-13`."*

Tiga bukti lain menguatkannya:

| Bukti | Isi |
| --- | --- |
| `validation-matrix.md` | `VAL-18` bertuliskan **"Berlaku pada: Menambah pemeriksaan"**. Ia memang tidak pernah mencakup pembatalan |
| `LAB-INH-001` | Alur pesanan **memuat** `CancelRequested` dan `Cancelled` sebagai pengecualian sah |
| `LAB-INH-006` | Sesudah `Requested`, dokter **mengajukan** pembatalan — jalurnya ada dan diatur, bukan ditutup |
| `LAB-INH-010` | Billing satu-satunya pemilik akibat finansial; koreksi tagihan memang bukan urusan Laboratorium |

---

## 3. Penahan — pertentangan antara `AC-55`/`AC-57` dan rancangan warisan

Acceptance criteria yang ditulis amandemen 2026-09-14 berbunyi:

| AC | Isi |
| --- | --- |
| `AC-55` | Menambah dan menghapus baris **ditolak sistem** setelah kelayakan wadah ditetapkan |
| `AC-57` | **Tidak ada satu pun jalan** untuk mengubah daftar pemeriksaan setelah kelayakan tagih terbit |

Keduanya bertentangan langsung dengan rancangan yang sudah berjalan dan dengan keputusan
warisan yang mengizinkan pembatalan terkendali sesudah titik tagih.

**Menambahkan penjagaan `VAL-18` ke `CancelAsync` berarti mengubah kebijakan pembatalan** —
bukan menambal kelalaian. Itu melampaui wewenang task ini, dan `SKILL.md` langkah 4
menyatakannya dengan tegas: *"menambah permukaan teknis boleh, mengarang kebijakan tidak."*

Karena itu **nol baris source diubah**, dan task ini dikembalikan sebagai `TERBLOKIR`.

### 3.1 Pertanyaan yang perlu dijawab pemilik modul

Tiga kemungkinan, dan ketiganya punya akibat berbeda:

| Pilihan | Akibat |
| --- | --- |
| **A. `AC-55`/`AC-57` dipersempit** menjadi "menambah baris" saja; pembatalan tetap terbuka dan dikoreksi Billing | Rancangan warisan utuh. `BR-34` perlu diamandemen, dan layar penerimaan perlu memberi tahu petugas bahwa pembatalan sesudah kelayakan **tetap menagih** sampai Billing mengoreksi |
| **B. `AC-55`/`AC-57` dipertahankan**; pembatalan sesudah kelayakan ditutup | Menyentuh `LAB-INH-001`, `LAB-INH-006`, dan `RJ-BIL-GATE-DEC-003` yang berstatus `locked-draft` milik blueprint `rawat-jalan`. **Perlu persetujuan pemilik `billing-kasir` dan `rawat-jalan`**, bukan pemilik Laboratorium sendirian |
| **C. Dibedakan menurut jalur masuk**, seperti `LAB-DEC-039` membedakan titik kunci | Pembatalan ditutup untuk penerimaan sampling langsung, tetap terbuka untuk pesanan dokter. Paling sesuai semangat `BR-34`, tetapi menambah satu percabangan perilaku yang harus terlihat di layar |

Rekomendasi pelapor: **A**, karena ia satu-satunya yang tidak menyentuh keputusan milik modul
lain — tetapi ini keputusan bisnis, dan tidak diambil dari sini.

---

## 4. Yang terbukti pada pemeriksaan ini

| AC | Hasil |
| --- | --- |
| `AC-55` | **Tidak terpenuhi.** Jalur tambah terjaga; jalur batal tidak, dan ketidakterjagaannya disengaja |
| `AC-56` | **Terpenuhi.** Penelusuran seluruh controller Laboratorium menemukan nol route bernama `process`, `diproses`, atau sejenisnya pada tingkat pemeriksaan |
| `AC-57` | **Tidak terpenuhi.** `POST /lab-examinations/{id}/cancel` adalah jalan yang dimaksud, dan ia terbuka |

### 4.1 Satu catatan tambahan untuk `AC-56`

Ada `PUT /lab-orders/{id}/start-process`, tetapi ia **bukan** yang dilarang `AC-56`: ia aksi
tingkat **pesanan** yang memindahkan status `Accepted → InProcess` sesuai `LAB-INH-001`, bukan
aksi tingkat pemeriksaan yang mengunci daftar.

Meski begitu namanya berdekatan dengan istilah "Pemeriksaan Diproses" pada `LAB-EVD-001`, dan
layar penerimaan perlu memastikan keduanya tidak tertukar. Dicatat agar tidak menjadi kekeliruan
yang mahal di kemudian hari.

---

## 5. Verifikasi

`AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`

| Jenis | Hasil |
| --- | --- |
| Backend Governance Preflight | **Lulus** |
| Review diff/scope | **Lulus — nol baris source diubah** |
| `dotnet build` | **Tidak dijalankan** untuk task ini. Tidak ada source yang disentuh, sehingga tidak ada yang perlu dibuktikan terkompilasi. Build terakhir repository pada sesi yang sama (`BE-LAB-20`) berstatus 0 Error, 0 Warning |
| Verifikasi kontrak API | **Lulus.** `VAL-18` tidak berubah bunyinya; tidak ada endpoint ditambah maupun dihapus |
| Verifikasi proses bisnis | **Terlaksana sebagai pemeriksaan**, dan hasilnya justru menemukan penahan |

---

## 6. Definition of Done

| Butir DoD | Status |
| --- | --- |
| Jalur tambah dan jalur hapus memakai penjagaan yang sama | **Tidak terpenuhi** — tertahan keputusan bisnis |
| `T-55d` hijau | **Tidak dibuat.** Menulis pengujian yang menegakkan `AC-57` berarti mengunci kebijakan yang belum diputuskan |
| Tidak ada aksi `Pemeriksaan Diproses` tersendiri yang dibuat | **Terpenuhi** |
| `VAL-18` tidak berubah bunyinya | **Terpenuhi** |

---

## 7. Catatan penutup

Roadmap menulis task ini *"boleh gagal pada percobaan pertama, dan itu memang gunanya"*. Ia
memang gagal — tetapi bukan dengan cara yang diperkirakan.

Yang diperkirakan: jalur hapus lupa diberi penjagaan, tinggal ditambal. Yang ditemukan: jalur
hapusnya bukan `DELETE`, penjagaannya sengaja tidak ada, dan acceptance criteria yang baru
ditulis kemarin justru bertentangan dengan keputusan warisan yang sudah dikunci modul lain.

Seandainya penjagaan itu ditambahkan begitu saja, hasilnya akan terlihat benar — build lulus,
pengujian hijau, task bertanda selesai — sementara satu jalur pembatalan yang sah menurut
`LAB-INH-006` diam-diam tertutup, dan koreksi tagihan yang menjadi wewenang Billing kehilangan
pintunya.

---

## 8. Penutupan — verifikasi akhir terhadap acceptance criteria yang diamandemen

Dijalankan 2026-09-14 setelah `LAB-DEC-049` mempersempit `AC-55` dan `AC-57`.

| AC (bunyi setelah amandemen) | Bukti pada `9067fa73` | Hasil |
|---|---|---|
| `AC-55a` — menambah baris **diizinkan** selama kelayakan belum ditetapkan | `LabExaminationService.AddAsync` hanya menjaga status pesanan, kepemilikan wadah, disiplin, keunikan, dan tarif. Tidak ada penghalang menyeluruh | ✅ |
| `AC-55b` / `AC-57` — menambah baris **ditolak** setelah kelayakan ditetapkan | `LabExaminationService.cs:123` — `if (specimen.SpecimenStatus is LabSpecimenStatus.Accepted or LabSpecimenStatus.Rejected)` → `409` `VAL-18` | ✅ |
| `AC-55c` — pembatalan **tidak ikut terkunci** | Penelusuran seluruh badan `CancelAsync`: **nol** rujukan `SpecimenStatus`. Yang menjaga hanya `VAL-19` (`Voided`) dan status `Cancelled` | ✅ |
| `AC-56` — tidak ada aksi `Pemeriksaan Diproses` tersendiri | Penelusuran seluruh controller Laboratorium: nol route bertingkat pemeriksaan bernama `process` atau sejenisnya. `PUT /lab-orders/{id}/start-process` adalah aksi tingkat **pesanan** sesuai `LAB-INH-001`, bukan pengunci daftar | ✅ |

### 8.1 Kenapa tidak ada source yang diubah, dan kenapa itu hasil yang benar

Task ini berdisposisi `EXISTING / REUSE` sejak awal. Setelah `LAB-DEC-049` mempersempit
cakupannya, **seluruh perilaku yang diminta ternyata sudah benar**:

- Yang harus dikunci, **sudah terkunci** — `VAL-18` pada jalur tambah.
- Yang tidak boleh dikunci, **memang tidak terkunci** — jalur batal.

Menambahkan kode pada keadaan seperti ini justru akan merusaknya.

### 8.2 Tentang "menulis penjaga `T-55e`"

Roadmap semula menuliskan cakupan ini sebagai "menulis pengujian penjaga". Setelah
`rules/backend/TEST_POLICY.md` dibaca, itu **tidak dikerjakan** dan memang tidak boleh:
backend tidak memelihara project automated test, dan membuatnya hanya sah bila pemilik meminta
secara eksplisit pada task aktif. Permintaan itu tidak ada.

`T-55b`, `T-55c`, `T-55e`, dan `T-56a` karena itu tetap berdiri pada
`testing/acceptance-test-matrix.md` sebagai **skenario verifikasi** yang dijalankan ketika
aplikasi diuji coba — bukan sebagai kode uji yang perlu ditulis sekarang. `T-56a` sudah
dijalankan pada task ini lewat penelusuran source, dan hasilnya tercatat di atas.

### 8.3 Satu pekerjaan yang berpindah ke frontend

`BR-44` menurunkan kewajiban baru yang **bukan** milik task ini: saat tombol batal ditekan
sesudah wadah dinyatakan `Layak`, layar wajib mengatakan bahwa tagihannya baru gugur setelah
Billing mengoreksi. Dicatat pada `03-frontend-architecture.md` bagian 10.5, dan menjadi cakupan
`FE-LAB-11`.

Tanpa kalimat itu, petugas akan menyangka pembatalan di layar sudah membatalkan tagihan — dan
mengetahuinya dari pasien yang protes di loket.
