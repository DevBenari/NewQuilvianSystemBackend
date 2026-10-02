# Tata Letak Riwayat pada Ruang Kerja Pemeriksaan IGD

| Field | Nilai |
| --- | --- |
| Tanggal | 16 September 2026 (keempat) |
| Jenis | Evidence tinjauan tampilan + audit source baca-saja. **Bukan** implementasi, **bukan** perubahan source |
| Skill | `plan-module-delivery` |
| Blueprint | `IGD-BP-001` revision `6` (`draft`); wewenang UI `03-frontend-architecture.md` bagian 11 dan 12.5 |
| Backend | `NewQuilvianSystemBackend` branch `rizkiG` `8544af1c` — **tidak disentuh gelombang ini** |
| Frontend | `QuilvianSystemFrontendDev` branch `RizkiV2` `36f122af9` |
| Pembanding | Quilvian V1, `C:\Users\BenariDev03\QuilvianV1\QuilvianSystemFrontendDev` branch `QuilvianSta` |
| Pelapor | Product/Domain Owner IGD, dari pemakaian layar Assesmen IGD |
| Batasan | Tanpa coding, migration, query basis data, build, test, commit |

---

## A. Kesimpulan eksekutif

**Riwayat tiap formulir pemeriksaan IGD ada, terisi benar, tetapi praktis tidak terbaca.** Pada
ruang kerja Pemeriksaan IGD, setiap tab menumpuk formulir panjang lebih dulu, lalu daftar riwayat
di bawahnya. Untuk sekadar membaca riwayat, perawat harus menggulir melewati seluruh isian —
pada tab Assesmen Awal IGD itu berarti tanda vital, pengkajian, pengkajian lanjutan, catatan
psikososial, catatan edukasi, dan blok penyelesaian.

Akibat kedua lebih halus dan lebih merugikan: **sesudah simpan berhasil, posisi gulir tidak
berpindah.** Formulir dikosongkan dan daftar riwayat dimuat ulang di bawah sana, tetapi perawat
tetap berada di area formulir. Yang terlihat hanyalah formulir yang tiba-tiba kosong — bentuk
umpan balik yang sama persis dengan kegagalan simpan.

Ini **bukan cacat data dan bukan cacat backend**. Daftarnya benar, muat ulangnya benar. Yang
kurang adalah susunan tampilannya.

Quilvian V1 tidak punya masalah ini karena memisahkan formulir dan riwayat menjadi dua tab, dan
memindahkan layar ke tab Riwayat setiap kali penyimpanan berhasil.

---

## B. Keadaan V2 apa adanya

Diperiksa pada `36f122af9`.

Ruang kerja dibangun oleh `emergency-assessment-detail-view.jsx`. Ia memasang **satu**
`role="tablist"` berisi tujuh tab Asuhan Keperawatan (baris 235-260), dan setiap tab mengisi satu
panel tunggal.

| Tab | Susunan sekarang | Berkas dan baris |
| --- | --- | --- |
| Assesmen Awal IGD | `EmergencyAssessmentFormCard` → `EmergencyAssessmentRecordTab` "Riwayat Assesmen Awal" | `emergency-assessment-initial-tab.jsx:136`, `:235` |
| SOAP | `EmergencyAssessmentFormCard` saja — **tanpa** riwayat | `emergency-assessment-soap-tab.jsx:96` |
| Nosokomial | `FormCard` → `Section` "Riwayat Kejadian Infeksi" | `emergency-assessment-nosocomial-tab.jsx:277`, `:414` |
| Catatan Terintegrasi | `Section` baca-saja | `emergency-assessment-integrated-note-tab.jsx:138` |
| Observasi | `FormCard` periode → `Section` daftar periode → `Section` primary survey → `FormCard` pemantauan → `Section` riwayat pemantauan | `emergency-assessment-observation-tab.jsx:876`, `:928`, `:1036`, `:1096`, `:1252` |
| Tindak Lanjut | `FormCard` → `Section` "Riwayat Tindak Lanjut" | `emergency-assessment-disposition-tab.jsx:319`, `:467` |
| Resep | `EmergencyAssessmentRecordTab` baca-saja | `emergency-assessment-detail-view.jsx:207` |

Di luar tujuh tab itu, nav kiri memuat **Penunjang Medis** (`:126`, `:157`) dan **Transfer Pasien**
(`:160`, `:184`) yang memakai susunan bertumpuk yang sama.

**Pola simpan pada tab Assesmen Awal**, `emergency-assessment-initial-tab.jsx:116-131`: sesudah
`createPatientAssessment.fulfilled`, formulir di-reset ke `EMPTY_SCREENING_FORM` lalu `reload()`
dipanggil. Tidak ada perpindahan fokus, tidak ada perpindahan gulir, tidak ada penanda baris baru.

---

## C. Keadaan V1 apa adanya

Diperiksa pada `C:\Users\BenariDev03\QuilvianV1\QuilvianSystemFrontendDev`, branch `QuilvianSta`.

Modul IGD V1 berada di `src/components/view/IGD/pengkajian/asuhan-keperawatan-igd/`. Setiap
sub-modul memakai satu pola yang sama:

| Sub-modul | Berkas tab | Isi |
| --- | --- | --- |
| Assesmen awal | `assesmen-awal-views/components/assesment-awal-tabs.jsx` | `Tabs` react-bootstrap berisi `form` dan `riwayat` |
| Observasi | `observasi/observasiIGD-tabs.jsx` | `Tabs` berisi `form` dan `riwayat` |
| Nosokomial, tindak lanjut | `nosokomial-views/`, `tindak-lanjut/` | susunan folder yang sama |

Perilaku yang menjadi inti tinjauan ini ada pada `assesment-awal-tabs.jsx` baris 13-20:

```jsx
const handleFormSubmitSuccess = (result) => {
  setActiveTab("riwayat");
  setRefreshHistoryTrigger((prev) => prev + 1);
};
const handleCreateNewAssessment = () => {
  setActiveTab("form");
};
```

Sesudah simpan berhasil, layar **berpindah sendiri** ke tab Riwayat dan daftarnya dimuat ulang;
dari sisi Riwayat ada jalan kembali ke Formulir.

> **Catatan penting soal branch.** Pemilik menyebut pola ini berasal dari branch `rizkiG`. Pada
> repository frontend V2, branch `rizkiG` **tidak memuat modul IGD sama sekali** — satu-satunya
> berkas ber-nama IGD di sana adalah `view/home/fitur-dashboard/igd/emergencyAlerts.jsx`. Pola
> yang dimaksud ada di **repository V1**, bukan di branch `rizkiG` milik V2. Ini selaras dengan
> catatan lama bahwa `rizkiG` pada lineage frontend sudah usang.

---

## D. Kenapa pola V1 tidak boleh disalin apa adanya

| No | Penghalang | Bukti |
| ---: | --- | --- |
| 1 | V2 sudah memasang satu `role="tablist"` untuk tujuh tab utama. Tab bersarang menjadikannya tablist di dalam tablist, dan relasi tab/panel terbaca dua kali oleh pembaca layar | `emergency-assessment-detail-view.jsx:238`; komentar `ClinicalSegmentedNav.jsx:6-14` yang menyatakan komponen itu dibuat **justru untuk menolak** penyarangan itu |
| 2 | V1 tidak punya nav kiri; V2 punya `ClinicalSectionNav`. Tab bersarang di V2 menjadi tingkat navigasi **ketiga** | `emergency-assessment-detail-view.jsx:383` |
| 3 | `Tabs` react-bootstrap adalah pustaka komponen yang gayanya tidak sebahasa dengan tab bar ruang kerja, dan penambahan pustaka komponen baru dilarang | `03-frontend-architecture.md:250` |

**Penggantinya sudah ada di repository dan tidak perlu dibuat:**

| Komponen | Jalur | Kegunaan pada gelombang ini |
| --- | --- | --- |
| `ClinicalSegmentedNav` | `src/components/ui/doctor-clinical-base/ClinicalSegmentedNav.jsx` | Segmen "Formulir \| Riwayat" sebagai grup radio, dengan dukungan badge angka bawaan |
| `ClinicalDataTable` | `src/components/ui/doctor-clinical-base/ClinicalDataTable.jsx` | Lembar pemantauan observasi berbentuk tabel, lengkap dengan judul, badge jumlah, dan empty state |
| `EmergencyAssessmentRecordTab` | `emergency-assessment-view/components/` | Kartu riwayat yang sudah dipakai — dipindah tempat, tidak ditulis ulang |

Preseden pemakaian `ClinicalSegmentedNav` di dalam satu tab klinis sudah ada pada
`inpatient-management/physician-workspace/tabs/medication-procedure/prescription-procedure-tab.jsx:88`.

---

## E. Kenapa Observasi ditangani terpisah

Tab Observasi **bukan** pasangan formulir–riwayat. Ia induk–anak bertingkat dua:

```text
periode observasi  ->  putaran pemantauan (banyak baris per periode)
```

Memaksanya menjadi dua segmen "Formulir | Riwayat" memutus hubungan itu: pemilihan periode
adalah prasyarat pencatatan maupun pembacaan pemantauan, sehingga ia tidak boleh berada di salah
satu sisi segmen.

Perbedaan kedua ada pada **cara membacanya**. Yang dicari perawat pada lembar pemantauan bukan
satu kejadian, melainkan arah perubahan — tekanan darah turun, nadi naik, saturasi turun.
Susunan kartu bertumpuk yang dipakai sekarang (`emergency-assessment-observation-tab.jsx:1267`)
memakai hampir satu layar untuk satu putaran, sehingga dua putaran berurutan tidak pernah
terlihat bersamaan. Bentuk berjajar menampilkan empat sampai enam putaran sekaligus.

Wewenang untuk mengubahnya sudah terdelegasi: `03-frontend-architecture.md:330` menyebut
penyajian tanda vital sebagai ringkasan satu baris **atau tabel** sebagai `DEV_DISCRETION`.

---

## F. Risiko yang dibawa keputusan ini, dan penambalnya

Menyembunyikan riwayat di balik segmen **memindahkan** risiko, tidak menghapusnya. Saat segmen
Formulir aktif, perawat tidak melihat bahwa rekannya baru saja mengkaji pasien yang sama. Di IGD,
satu bed dapat disentuh dua perawat dalam satu giliran, dan pengkajian ganda menghasilkan dua
dokumen sah yang saling bertentangan.

V1 membiarkan lubang ini terbuka. Penambalnya pada gelombang ini adalah **baris "terakhir
dikaji"** yang tetap terlihat saat segmen Formulir aktif, berisi waktu, nama pencatat, dan
ringkasan singkat pengkajian terakhir.

Baris itu **wajib** dibentuk dari daftar riwayat yang memang sudah dimuat layar. Menambah
endpoint, ruas data, atau sumber data baru untuknya melanggar penguncian
`03-frontend-architecture.md:331`, yang menyatakan isi data dan sumber datanya **bukan**
`DEV_DISCRETION`.

---

## G. Wewenang UI — sudah ada, tidak perlu approval baru

| Hal | Wewenang | Bukti |
| --- | --- | --- |
| Bentuk tab, modal, atau drawer | `DEV_DISCRETION` | `03-frontend-architecture.md:247` |
| Bentuk pemilihan "catat baru" lawan "pilih yang sudah ada" — radio, tab, atau tombol | `DEV_DISCRETION`, mengikuti komponen yang sudah ada | `:328` |
| Susunan kolom riwayat dan urutannya | `DEV_DISCRETION` | `:329` |
| Tanda vital sebagai ringkasan satu baris atau tabel | `DEV_DISCRETION` | `:330` |
| Isi data yang ditampilkan, sumber datanya, aturan 12.4 | **Bukan** `DEV_DISCRETION` — dikunci | `:331` |
| Palet warna baru | **Dilarang** — salin dari `emergency-triage.module.css` | `:249` |
| Pustaka komponen baru | **Dilarang** — pakai `form-pemeriksaan-ui` dan `base-features` | `:250` |

---

## H. Tiga opsi yang diajukan, dan yang dipilih

| Opsi | Bentuk | Putusan pemilik |
| --- | --- | --- |
| A | Segmen "Formulir \| Riwayat" memakai `ClinicalSegmentedNav`, berpindah sendiri sesudah simpan, ditambah baris "terakhir dikaji" | **Dipilih** 16 September 2026 |
| B | Dua kolom — formulir kiri, riwayat menempel kanan | Ditolak: nav kiri sudah memakan lebar, dan pada 1366 px susunannya jatuh menumpuk kembali |
| C | Susunan dibiarkan, ditambah tombol jangkar ke daftar riwayat | Ditolak: tidak menyelesaikan keluhan aslinya |

Keputusan tambahan pada sesi yang sama:

- **Observasi dikerjakan sekalian**, pada gelombang yang sama, sebagai kartu task terpisah.
- **SOAP dibiarkan** seperti sekarang, karena riwayatnya memang milik tab Catatan Terintegrasi.
- **Catatan Terintegrasi dan Resep** tetap daftar baca-saja tanpa segmen.
- Baris **"terakhir dikaji" ikut dibangun**, tidak ditunda.

Mockup ketiga opsi beserta tata letak Observasi: <https://claude.ai/artifact/67Fb96cUSzBEg7TsguULk2>

---

## I. Yang **tidak** berubah

| Hal | Keterangan |
| --- | --- |
| Backend | Nol perubahan, nol endpoint baru, nol kenaikan versi kontrak |
| Isi dan sumber data | Setiap ruas yang tampil hari ini tetap tampil, dari sumber yang sama |
| Aksi Selesaikan dan isian Kesimpulan observasi | Milik `FE-IGD-024`; tidak boleh berubah |
| Penautan tanda vital pada pemantauan | Milik `FE-IGD-028`; tidak boleh berubah |
| Tab SOAP, Catatan Terintegrasi, Resep | Tidak disentuh |
| CSS global dan palet | Tidak disentuh |

---

## J. Keputusan yang lahir dari tinjauan ini

| ID | Isi singkat |
| --- | --- |
| `IGD-DEC-133` | Segmen Formulir/Riwayat memakai `ClinicalSegmentedNav`; berpindah sendiri sesudah simpan; baris "terakhir dikaji" pada Assesmen Awal; SOAP, Catatan Terintegrasi, dan Resep dikecualikan |
| `IGD-DEC-134` | Tata letak Observasi: pemilih periode di atas, primary survey diringkas, lembar pemantauan berbentuk tabel, segmen "Lembar Pemantauan \| Catat Pemantauan" |

Task yang dibentuk: `FE-IGD-031` dan `FE-IGD-032` pada
[frontend-roadmap.md](../roadmap/frontend-roadmap.md) bagian R3.9.
