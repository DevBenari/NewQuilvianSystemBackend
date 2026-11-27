# ISSUE-008 — Menu Unit Organisasi dan Struktur Organisasi Tumpang-Tindih, Departemen dan Posisi Dikelola dari Layar Struktur

```yaml
issue_id: ISSUE-HRD-008
module_id: human-resource
submodule: null   # blueprint berbentuk SINGLE (HRD-DEC-003); blueprint root = human-resource/
layar: "Master Data SDM — Organisasi & Struktur Jabatan: Struktur Organisasi (/hr/master-data/organization) dan Unit Organisasi (/hr/master-data/organization-unit)"
sumber_laporan: "Masukan analis bisnis yang diteruskan pemilik, 24-11-2026: 3 kalimat, tanpa lampiran. Pemilik memilih Opsi A pada percakapan yang sama."
tanggal_issue: "2026-11-24"
status: SELESAI
keparahan_tertinggi: Medium
source_sha_backend: "3ef27c96 (MHamzah)"
source_sha_frontend: "fe8aebf2f (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-008-gabung-unit-organisasi-ke-struktur-organisasi.md
```

---

## 1. Ringkasan

Masukan ini menyangkut dua menu di **Sumber Daya Manusia > Master Data > Organisasi & Struktur
Jabatan**, yaitu **Struktur Organisasi** dan **Unit Organisasi**. Sekarang petugas HR melihat
pohon organisasi rumah sakit di menu Struktur Organisasi. Tetapi untuk menambah atau mengubah
unitnya, petugas harus pindah ke menu Unit Organisasi. Di sisi lain, menu Struktur Organisasi
justru dipakai untuk menambah **Departemen** dan **Posisi**, padahal keduanya bukan "struktur".

Analis meminta tiga hal. Menu Unit Organisasi tidak dipakai lagi. Isinya dipindah ke menu
Struktur Organisasi. Departemen dan Posisi ditambahkan dari master masing-masing. Pemilik
memilih **Opsi A** pada 24-11-2026: Departemen dan Posisi masing-masing mendapat menu sendiri,
dan menu Struktur Organisasi berisi pohon serta pengelolaan unit.

Kedua butir laporan ternyata **sesuai desain Menu 05 yang disetujui 16 November 2026**. Karena
itu jenisnya `DESIGN_CHANGE`: dokumen desain dan source harus diubah bersama. Selama penelusuran
juga ditemukan lima masalah tambahan pada layar yang sama. Yang paling terasa: tombol "Lihat
Detail Unit Lengkap" di pohon tidak membuka unit yang dipilih, dan isian "Level Hierarki" wajib
diisi padahal nilainya diabaikan server.

Tidak ada butir yang membuat alur berhenti total. Keparahan tertinggi `Medium`.

---

## 2. Laporan Asli

| No. laporan | Keluhan (kata-kata pelapor) | Lampiran |
| :---: | :--- | :--- |
| 1 | "oke, berarti unit organisasi ini kayaknya gak usah dipake menunya" | Tidak ada |
| 2 | "isinya pindahin ke menu struktur organisasi" | Tidak ada |
| 3 | "untuk nambahin departement, posisi itu di masternya masing-masing" | Tidak ada |

Konteks percakapan 24-11-2026:

1. Pemilik meneruskan tiga kalimat analis dan bertanya apakah maksudnya dipahami.
2. Agent menjelaskan kondisi kedua layar dan menawarkan dua tafsiran untuk kalimat 3:
   - **Opsi A**: Departemen dan Posisi masing-masing mendapat menu sendiri.
   - **Opsi B**: Departemen dan Posisi tetap berupa tab di Struktur Organisasi.
3. Pemilik memilih **Opsi A**, lalu meminta rencana perbaikan.

---

## 3. Ringkasan Temuan

| ID | No. laporan | Judul | Jenis | Area | Keparahan | Status bukti | Perbaikan |
| :--- | :---: | :--- | :---: | :---: | :---: | :---: | :--- |
| `ISS-HRD-008-01` | 1, 2 | Isi struktur organisasi terpecah ke dua menu | `DESIGN_CHANGE` | Frontend | Medium | SUDAH-VERIFIKASI | `FIX-HRD-008-01`, `FIX-HRD-008-03`, `FIX-HRD-008-04` |
| `ISS-HRD-008-02` | 3 | Departemen dan Posisi ditambah dari layar Struktur Organisasi, tanpa menu sendiri | `DESIGN_CHANGE` | Frontend | Medium | SUDAH-VERIFIKASI | `FIX-HRD-008-01`, `FIX-HRD-008-02`, `FIX-HRD-008-04`, `FIX-HRD-008-05` |
| `ISS-HRD-008-T1` | — | Tombol "Lihat Detail Unit Lengkap" di pohon membuka daftar penuh, bukan unit yang dipilih | `BUG` | Frontend | Medium | SUDAH-VERIFIKASI | `FIX-HRD-008-03` |
| `ISS-HRD-008-T2` | — | Isian "Level Hierarki" wajib diisi, tetapi nilainya diabaikan server | `BUG` | Frontend | Medium | SUDAH-VERIFIKASI | `FIX-HRD-008-05` |
| `ISS-HRD-008-T3` | — | Detail unit menampilkan ID mentah, bukan nama | `RULE_VIOLATION` | Backend + Frontend | Medium | SUDAH-VERIFIKASI | `FIX-HRD-008-06`, `FIX-HRD-008-07` |
| `ISS-HRD-008-T4` | — | Teks Inggris dan istilah teknis pada layar yang dipindah | `RULE_VIOLATION` | Frontend | Low | SUDAH-VERIFIKASI | `FIX-HRD-008-02`, `FIX-HRD-008-03`, `FIX-HRD-008-05` |
| `ISS-HRD-008-T5` | — | Server tidak memeriksa apakah Departemen Referensi ada dan aktif | `BUG` | Backend | Low | SUDAH-VERIFIKASI; akibat runtime `DUGAAN` | `FIX-HRD-008-08` |

---

## 4. Rincian per Temuan

### ISS-HRD-008-01 — Isi struktur organisasi terpecah ke dua menu

| | |
| :--- | :--- |
| **No. laporan** | 1, 2 |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source frontend, source backend, dan dokumen desain Menu 05 |
| **Perbaikan** | `FIX-HRD-008-01`, `FIX-HRD-008-03`, `FIX-HRD-008-04` |

**Apa yang terjadi.** Petugas HR punya dua menu dengan nama hampir sama. Menu **Struktur
Organisasi** punya tombol **Hierarki** yang menampilkan pohon unit. Pohon itu hanya bisa
dilihat, tidak bisa diubah. Untuk menambah, mengubah, atau menonaktifkan unit, petugas harus
membuka menu lain, yaitu **Unit Organisasi**. Data yang tampil di kedua menu sama persis.

**Kenapa terjadi.**

```text
Petugas harus berpindah menu untuk mengurus satu hal yang sama
  ← sebab langsung: sidebar memuat dua butir terpisah untuk satu data
       menu-items.jsx:233-238  "Struktur Organisasi" → /hr/master-data/organization
       menu-items.jsx:251-256  "Unit Organisasi"     → /hr/master-data/organization-unit
  ← halaman Struktur Organisasi meminjam data Unit Organisasi untuk mode Hierarki,
    lalu melempar petugas ke menu Unit Organisasi untuk melihat detail
  ← akar: desain Menu 05 (16-11-2026) memang menetapkan "Unit Kerja" dan
    "Struktur Organisasi" sebagai dua submenu, dengan pohon hanya untuk dilihat
```

Bukti di source frontend (SHA `fe8aebf2f`):

`src/components/view/hr/master-data/organization/master-data-organization-view.jsx:364-389`
memasang tombol `Daftar | Hierarki`. Mode Hierarki menampilkan `OrganizationHierarchyView`.

`src/components/view/hr/master-data/organization/hierarchy/organization-hierarchy-view.jsx:10-11`
mengambil data dari endpoint milik Unit Organisasi:

```js
const HIERARCHY_ENDPOINT =
  "/v1/corporate/human-resource/master-data/organization-units/hierarchy";
```

`organization-hierarchy-view.jsx:178-183` mengirim petugas ke menu lain:

```js
const handleNavigateToUnitDetail = () => {
  if (!selectedNode?.unitCode) return;
  router.push(
    `/hr/master-data/organization-unit?search=${encodeURIComponent(selectedNode.unitCode)}`,
  );
};
```

Bukti di source backend (SHA `3ef27c96`). Kedua menu dijaga **hak akses yang berbeda**:

| Controller | Route | Hak akses (`[AccessPermission]`) | Bukti |
| --- | --- | --- | --- |
| `OrganizationController` | `api/v1/corporate/human-resource/master-data/organization` | `Organization : Read/Create/Update/Delete` | `OrganizationController.cs:26-36`, `137-140`, `351-355` |
| `OrganizationUnitController` | `api/v1/corporate/human-resource/master-data/organization-units` | `OrganizationUnit : Read/Create/Update/Delete` | `OrganizationUnitController.cs:22-31`, `265-267`, `403-405` |

Akibatnya, halaman Struktur Organisasi sekarang mencampur dua hak akses. Mode Daftar memakai
`Organization : Read`, sedangkan mode Hierarki memakai `OrganizationUnit : Read`.

**Apakah ini menyimpang dari desain?** Tidak. Perilaku ini **sesuai** desain yang disetujui,
jadi jenisnya `DESIGN_CHANGE`. Rujukannya ada di
`docs/update/struktur-organisasi/INSTRUKSI_AGENT_HRD_MENU_05_STRUKTUR_ORGANISASI_DAN_JABATAN.md`:

| Baris | Isi desain yang disetujui |
| --- | --- |
| 5 | Menu 05 punya 6 submenu, termasuk "Unit Kerja" dan "Struktur Organisasi" secara terpisah |
| 59 | `DEC-M05-03`: mode Hierarki adalah tambahan; mode daftar existing tetap dipertahankan |
| 80 | Unit Kerja: "pertahankan route `/hr/master-data/organization-unit`" |
| 86-91 | Struktur Organisasi: tab Department/Position tidak boleh dihapus; pohon hanya dibaca; ubah unit lewat Unit Kerja |
| 213-216 | Peta layar: Unit Kerja dan Struktur Organisasi dua route terpisah |
| 279-300 | Wireframe "Manajemen Organization [Daftar] [Hierarki]" |

Laporan implementasi Menu 05 mencatat kedua layar sudah dibangun persis begitu:
`LAPORAN_IMPLEMENTASI_HRD_MENU_05_STRUKTUR_ORGANISASI_DAN_JABATAN.md:25-26`.

**Dampak nyata.** Contoh: rumah sakit membuka **Instalasi Farmasi** baru di bawah **Direktorat
Penunjang Medis**.

1. Petugas HR membuka Struktur Organisasi > Hierarki untuk melihat letak direktorat itu.
2. Petugas pindah ke menu Unit Organisasi, menekan "+ Tambah Unit Organisasi", lalu menyimpan.
3. Petugas kembali ke Struktur Organisasi > Hierarki dan menekan Refresh untuk memastikan
   Instalasi Farmasi sudah muncul di tempat yang benar.

Petugas baru tidak tahu menu mana yang harus dibuka. Ada juga dampak hak akses (status bukti:
SUDAH-VERIFIKASI di kode, belum diuji di browser). Peran yang hanya punya `Organization : Read`
bisa membuka halaman Struktur Organisasi, tetapi mode Hierarki gagal dengan kode 403. Artinya
pengguna tidak punya hak akses untuk data itu.

**Rekomendasi.** Opsi A, yang sudah dipilih pemilik. Menu **Struktur Organisasi** memakai
halaman unit yang sudah ada. Halaman itu ditambah tombol `Daftar | Hierarki`, dan pohonnya
dipindah ke sana. Menu Unit Organisasi dihapus dari sidebar. Rinciannya ada di plan repair.

---

### ISS-HRD-008-02 — Departemen dan Posisi ditambah dari layar Struktur Organisasi, tanpa menu sendiri

| | |
| :--- | :--- |
| **No. laporan** | 3 |
| **Jenis** | `DESIGN_CHANGE` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source frontend dan dokumen desain Menu 05 |
| **Perbaikan** | `FIX-HRD-008-01`, `FIX-HRD-008-02`, `FIX-HRD-008-04`, `FIX-HRD-008-05` |

**Apa yang terjadi.** Saat petugas membuka menu Struktur Organisasi, yang tampil pertama adalah
tab **Departemen** dan **Posisi** beserta tombol "+ Tambah Departemen" dan "+ Tambah Posisi".
Jadi menu "Struktur Organisasi" sebenarnya bekerja sebagai master Departemen dan master Posisi.
Kedua master itu tidak punya menu sendiri. Padahal master lain di grup yang sama punya menu
sendiri, misalnya Rumpun Jabatan, Level Jabatan, dan Grade Karyawan.

**Kenapa terjadi.**

```text
Departemen dan Posisi tidak terlihat sebagai master yang berdiri sendiri
  ← sebab langsung: keduanya hanya berupa tab di halaman /hr/master-data/organization
  ← akar: desain Menu 05 meminta tab Department/Position dipertahankan di halaman
    Struktur Organisasi
```

`src/lib/constants/hr/master-data/organization/organization-constants.jsx:1-15` mendefinisikan
dua tab:

```js
export const ORGANIZATION_TABS = {
  DEPARTMENTS: "departments",
  POSITIONS: "positions",
};

export const ORGANIZATION_TAB_ITEMS = Object.freeze([
  { key: ORGANIZATION_TABS.DEPARTMENTS, label: "Departemen" },
  { key: ORGANIZATION_TABS.POSITIONS, label: "Posisi" },
]);
```

`master-data-organization-view.jsx:398-423` memasang tab dan tombol tambah.
`src/lib/hooks/hr/master-data/organization/use-master-data-organization.jsx:315-317` membaca
tab aktif dari alamat `?type=`. Baris 775 membuka form tambah di
`/hr/master-data/organization/{type}/create`.

Ada sebab kedua yang membuat petugas bingung. Kata **"Departemen"** muncul dengan dua arti:

| Di mana | Artinya | Bukti |
| --- | --- | --- |
| Tab Departemen di Struktur Organisasi | Master departemen (`MstDepartment`), daftar nama departemen yang dipakai data pegawai dan hak akses | `organization-constants.jsx:8-9` |
| Tipe Unit "Departemen" di form Unit Organisasi | Salah satu tingkat dalam pohon unit | `organization-unit-constants.jsx:22-30` |
| Isian "Departemen Referensi" di form Unit Organisasi | Penghubung dari unit ke master departemen | `organization-unit-constants.jsx:71-80`; `MstOrganizationUnit.cs:19-20` ("Bridge ke master department lama") |

**Apakah ini menyimpang dari desain?** Tidak, jadi jenisnya `DESIGN_CHANGE`. Rujukannya ada di
`INSTRUKSI_AGENT_HRD_MENU_05_STRUKTUR_ORGANISASI_DAN_JABATAN.md`:

- baris 86–88: "Mode Daftar mempertahankan tab Department/Position existing";
- baris 102: "pertahankan tab Position pada `/hr/master-data/organization`".

**Dampak nyata.** Contoh: petugas baru diminta menambah posisi "Kepala Instalasi Farmasi". Ia
mencari menu "Posisi" atau "Jabatan" di sidebar dan tidak menemukannya. Posisi ternyata
disembunyikan sebagai tab di dalam menu Struktur Organisasi. Saat membuat unit, petugas juga
tidak tahu apakah departemen harus dibuat dari form unit atau dari tempat lain.

**Rekomendasi.** Opsi A, yang sudah dipilih pemilik.

1. **Departemen** dan **Posisi** masing-masing mendapat menu sendiri. Layar daftar, tambah,
   detail, dan ubah tetap memakai yang sudah ada.
2. Form unit di Struktur Organisasi hanya **memilih** departemen yang sudah ada. Form unit
   sekarang memang sudah berupa daftar pilihan. Yang ditambahkan adalah petunjuk bahwa
   departemen baru dibuat di menu Departemen.

---

## 5. Tanya-jawab Pelapor

> **T:** "Apakah anda mengerti yang dimaksud?" (pertanyaan pemilik tentang tiga kalimat
> analis)
>
> **J:** Ya. Ada tiga maksud. Menu Unit Organisasi dihapus dari sidebar. Isinya, yaitu pohon
> unit beserta tambah dan ubah unit, dipindah ke menu Struktur Organisasi. Departemen dan Posisi
> tidak lagi dibuat dari layar Struktur Organisasi, tetapi dari master masing-masing. Satu-satunya
> bagian yang perlu dipastikan adalah apakah "master masing-masing" berarti menu sendiri. Pemilik
> sudah menjawabnya dengan memilih Opsi A: ya, menu sendiri.

Analis sendiri tidak mengajukan pertanyaan eksplisit.

---

## 6. Temuan Tambahan

### ISS-HRD-008-T1 — Tombol "Lihat Detail Unit Lengkap" di pohon membuka daftar penuh, bukan unit yang dipilih

| | |
| :--- | :--- |
| **No. laporan** | — |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source; belum diuji di browser |
| **Perbaikan** | `FIX-HRD-008-03` |
| **Kenapa dicantumkan** | Tombol ini ikut pindah ke halaman Struktur Organisasi yang baru, jadi harus diperbaiki sekaligus |

**Apa yang terjadi.** Petugas memilih "Instalasi Farmasi" di pohon, lalu menekan "Lihat Detail
Unit Lengkap". Yang terbuka adalah daftar **semua** unit. Detail Instalasi Farmasi tidak
terbuka, dan daftar juga tidak tersaring. Petugas harus mencari ulang.

**Kenapa terjadi.** Tombol mengirim kode unit lewat alamat
(`organization-hierarchy-view.jsx:180-182`, `?search=<kode>`). Tetapi hook daftar unit
`src/lib/hooks/hr/master-data/organization-unit/use-master-data-organization-unit.jsx` tidak
membaca alamat sama sekali, karena tidak ada `useSearchParams` di berkas itu. Kata pencarian
hanya datang dari isian pencarian di layar (baris 65, 270, dan 319). Jadi `?search=` diabaikan.

Hook yang sama sudah punya cara resmi membuka detail, yaitu `openDetail` di baris 362-378.
Cara itu membuat token rute privat lalu membuka `/hr/master-data/organization-unit/{token}`.

**Apakah ini menyimpang dari desain?** Ya, jadi `BUG`. Desain Menu 05 baris 304 meminta tombol
itu "navigasi ke detail Unit Kerja", bukan ke daftar.

**Dampak nyata.** Dengan 40 unit, petugas harus mengetik ulang "Farmasi" di isian pencarian lalu
klik dua kali barisnya. Itu dua langkah tambahan setiap kali memeriksa satu unit dari pohon.

**Rekomendasi.** Tombol di panel pohon membuka halaman detail unit secara langsung, memakai
mekanisme token yang sama dengan `openDetail`.

---

### ISS-HRD-008-T2 — Isian "Level Hierarki" wajib diisi, tetapi nilainya diabaikan server

| | |
| :--- | :--- |
| **No. laporan** | — |
| **Jenis** | `BUG` |
| **Area** | Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source frontend dan backend |
| **Perbaikan** | `FIX-HRD-008-05` |
| **Kenapa dicantumkan** | Form ini menjadi form utama menu Struktur Organisasi; isian yang menyesatkan ikut terbawa |

**Apa yang terjadi.** Form tambah dan ubah unit mewajibkan isian "Level Hierarki", minimal 1.
Apa pun yang diketik petugas, server menghitung levelnya sendiri dari unit induk, lalu
menyimpan hasil hitungannya.

**Kenapa terjadi.**

`src/lib/constants/hr/master-data/organization-unit/organization-unit-constants.jsx:99-109`:

```js
{
  name: "levelNumber",
  label: "Level Hierarki",
  type: "number",
  placeholder: "Contoh: 1",
  description:
    "Level minimal 1. Sesuaikan dengan kedalaman hierarki organisasi.",
  required: true,
  min: 1,
  defaultValue: "1",
},
```

`Areas/Corporate/HumanResource/MasterData/Organization/Controllers/OrganizationUnitController.cs:418-440`
saat membuat unit:

```csharp
var parentId = NormalizeGuid(request.ParentOrganizationUnitId);
int calculatedLevel = 1;
if (parentId.HasValue)
{
    var parentLevel = await _dbContext.Set<MstOrganizationUnit>()
        ...
        .Select(x => x.LevelNumber)
        .FirstOrDefaultAsync();
    calculatedLevel = parentLevel + 1;
}
...
LevelNumber = calculatedLevel,
```

Saat mengubah unit, server melakukan hal yang sama di baris 505-516. Penghitungan otomatis ini
memang keputusan `FIN-HRD-05-BE-001` ("automatic LevelNumber calculation",
`LAPORAN_IMPLEMENTASI_HRD_MENU_05_STRUKTUR_ORGANISASI_DAN_JABATAN.md:21`). Formnya saja yang
tidak ikut disesuaikan.

**Apakah ini menyimpang dari desain?** Ya, jadi `BUG`. Desain Menu 05 baris 76 berbunyi
"`LevelNumber` tidak boleh menjadi angka arbitrer yang bertentangan dengan hierarchy". Form
yang meminta angka bebas bertentangan dengan aturan itu.

**Dampak nyata.** Contoh: Direktorat Penunjang Medis berada di level 2. Petugas membuat
Instalasi Farmasi di bawahnya dan mengetik level **5**. Server menyimpan level **3**, yaitu
2 + 1. Halaman detail lalu menampilkan "Level Hierarki: 3". Petugas mengira datanya berubah
sendiri.

**Rekomendasi.** Isian level dihapus dari form. Sebagai gantinya tampil keterangan: "Level
ditentukan otomatis dari unit induk." Level tetap tampil di daftar, detail, dan pohon.

---

### ISS-HRD-008-T3 — Detail unit menampilkan ID mentah, bukan nama

| | |
| :--- | :--- |
| **No. laporan** | — |
| **Jenis** | `RULE_VIOLATION` |
| **Area** | Backend + Frontend |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di source frontend dan backend |
| **Perbaikan** | `FIX-HRD-008-06`, `FIX-HRD-008-07` |
| **Kenapa dicantumkan** | Halaman detail ini dibuka dari pohon Struktur Organisasi setelah perbaikan T1 |

**Apa yang terjadi.** Halaman detail unit menampilkan baris seperti "ID Legal Entity:
3f2a9c1e-…", "ID Fasilitas Rumah Sakit: …", "ID Parent Unit: …", dan "ID Departemen: …".
Petugas tidak bisa membaca ke departemen mana unit itu terhubung.

**Kenapa terjadi.** `organization-unit-constants.jsx:377-396` memang menampilkan kolom ID:

```js
{ name: "legalEntityId", label: "ID Legal Entity", dataKeys: ["legalEntityId", "LegalEntityId"] },
{ name: "hospitalSiteId", label: "ID Fasilitas Rumah Sakit", dataKeys: ["hospitalSiteId", "HospitalSiteId"] },
{ name: "parentOrganizationUnitId", label: "ID Parent Unit", dataKeys: ["parentOrganizationUnitId", "ParentOrganizationUnitId"] },
{ name: "departmentId", label: "ID Departemen", dataKeys: ["departmentId", "DepartmentId"] },
```

Akarnya ada di respons server. `Areas/Corporate/HumanResource/MasterData/Organization/DTOs/OrganizationUnitDtos.cs:15-44`
hanya membawa nama untuk unit induk (`ParentUnitName`). Nama legal entity, fasilitas, dan
departemen tidak dikirim. Akibatnya layar hanya punya ID-nya.

**Apakah ini menyimpang dari desain?** Ya, jadi `RULE_VIOLATION`. Ini melanggar daftar periksa
`diagnose-module-issue` ("pengenal teknis tampil di layar") dan aturan output 5.3 ("jangan
menampilkan UUID mentah sebagai satu-satunya penjelas").

**Dampak nyata.** Auditor yang memeriksa "unit ini milik departemen apa" harus menyalin ID lalu
mencarinya di menu Departemen satu per satu.

**Rekomendasi.** Server menambahkan field nama ke respons detail. Field yang sudah ada tidak
diubah, jadi pemakai lain tidak terganggu. Layar lalu menampilkan nama dan membuang baris ID.

---

### ISS-HRD-008-T4 — Teks Inggris dan istilah teknis pada layar yang dipindah

| | |
| :--- | :--- |
| **No. laporan** | — |
| **Jenis** | `RULE_VIOLATION` |
| **Area** | Frontend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI di source |
| **Perbaikan** | `FIX-HRD-008-02`, `FIX-HRD-008-03`, `FIX-HRD-008-05` |
| **Kenapa dicantumkan** | Seluruh teks ini ada di layar yang disentuh perbaikan ini |

**Apa yang terjadi.** Layar mencampur Bahasa Inggris dan istilah teknis:

| Teks sekarang | Lokasi |
| --- | --- |
| "Manajemen Organization", "Data Organization", "Mengambil data organization...", "Data organization tidak ditemukan." | `master-data-organization-view.jsx:71-75`, `366`, `483-484` |
| "Daftar Organization" | `organization-constants.jsx:20`, `28` |
| Judul tab peramban "Master Data Organization", "Tambah Organization" | `src/app/hr/master-data/organization/page.jsx:4`; `[type]/create/page.jsx:4` |
| "Refresh", "Parent Unit", "Root (Unit Teratas)" | `organization-hierarchy-view.jsx:210`, `289`, `294` |
| Tipe unit tampil mentah, misalnya "Installation" | `organization-hierarchy-view.jsx:277` |
| "Parent Unit Organisasi", "Parent Unit", "Child", "Root Unit" | `organization-unit-constants.jsx:63`, `258`, `288`, `300` |

Pesan dari server seperti "Organization unit berhasil dibuat." dan "Unit type tidak valid."
(`OrganizationUnitController.cs:469`, `726`) juga berbahasa campur. Pesan server itu **tidak**
dimasukkan ke rencana ini supaya cakupannya tetap kecil, dan hanya dicatat di sini.

**Apakah ini menyimpang dari desain?** Ya, jadi `RULE_VIOLATION`. Ini melanggar daftar periksa
"teks Inggris atau pesan teknis mentah".

**Rekomendasi.** Teks layar diganti ke Bahasa Indonesia, sekaligus dengan perbaikan yang memang
menyentuh berkas tersebut. Contohnya "Unit Induk" untuk "Parent Unit", "Sub-Unit" untuk "Child",
dan "Muat Ulang" untuk "Refresh".

---

### ISS-HRD-008-T5 — Server tidak memeriksa apakah Departemen Referensi ada dan aktif

| | |
| :--- | :--- |
| **No. laporan** | — |
| **Jenis** | `BUG` |
| **Area** | Backend |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI (pemeriksaan tidak ada); akibat runtime `DUGAAN` |
| **Perbaikan** | `FIX-HRD-008-08` |
| **Kenapa dicantumkan** | Setelah perubahan ini, "departemen dipilih dari master" menjadi aturan utama. Server harus ikut menjaga aturan itu |

**Apa yang terjadi.** Saat unit disimpan, server memeriksa legal entity, fasilitas, dan unit
induk. Departemen Referensi tidak diperiksa.

**Kenapa terjadi.** `OrganizationUnitController.cs:715-804` (`ValidateRequestAsync`) tidak
memuat pemeriksaan `DepartmentId`. Nilainya langsung disimpan di baris 436 dan 513. Di database
ada relasi wajib ke `MstDepartment`
(`Migrations/20260726161839_initializeBigModulHRD2.cs:10031`,
`FK_MstOrganizationUnit_MstDepartment_DepartmentId`).

**Dampak nyata.** Lewat layar, risikonya kecil karena daftar pilihan departemen hanya memuat
yang aktif (`OrganizationController.cs:227`, `onlyActive = true`). Lewat permintaan langsung ke
API, ada dua kemungkinan:

- departemen yang sudah nonaktif tetap diterima;
- ID departemen yang tidak ada ditolak database. `DUGAAN`: penolakan itu muncul sebagai galat
  umum (kode 500), bukan pesan yang jelas.

Cara membuktikan: di lingkungan pengembangan, kirim `POST .../organization-units` dengan
`departmentId` acak, lalu catat kode status dan pesannya.

**Rekomendasi.** Server menolak departemen yang tidak ada atau nonaktif dengan kode 400 dan
pesan "Departemen tidak ditemukan atau tidak aktif." Kode 400 berarti isian yang dikirim
salah.

---

## 7. Pertanyaan dan Keputusan yang Dibutuhkan

**Untuk pelapor:** Tidak ada. Ketiga kalimat analis sudah jelas setelah pemilik memilih Opsi A.

**Keputusan yang sudah diambil:**

| No | Keputusan | Diambil oleh | Tanggal |
| ---: | --- | --- | --- |
| KD-01 | Opsi A: Departemen dan Posisi masing-masing mendapat menu sendiri. Struktur Organisasi berisi pohon dan pengelolaan unit. Menu Unit Organisasi dihapus dari sidebar. | Pemilik, atas masukan analis | 2026-11-24 |

**Untuk pemilik:**

| No | Keputusan | Pilihan | Rekomendasi | Menahan perbaikan |
| ---: | --- | --- | --- | --- |
| K-01 *(diputuskan 24-11-2026: (a), `HRD-DEC-258`; lama `HRD-DEC-249`)* | Label hak akses di layar Akses Role masih "Organization" (dipakai menu Departemen dan Posisi) dan "Organization Unit" (dipakai menu Struktur Organisasi). Perlu diganti? | (a) Biarkan dulu; (b) Ganti label tampilan di atribut `[AccessController]` backend menjadi "Departemen & Posisi" dan "Struktur Organisasi" | (a). Admin cukup diberi catatan pemetaan di panduan. Mengganti label menyentuh atribut backend, dan dampaknya pada sinkronisasi daftar hak akses perlu dicek tersendiri. | Tidak ada |
| K-02 *(diputuskan 24-11-2026: (a), `HRD-DEC-259`; lama `HRD-DEC-250`)* | Tipe unit "Departemen" di Struktur Organisasi tetap dipakai? | (a) Tetap, dan unit bertipe Departemen boleh dihubungkan ke master Departemen; (b) Hapus tipe Departemen dari pilihan | (a). Rumah sakit umum memang punya tingkat departemen, misalnya Departemen Keperawatan, dan data lama mungkin sudah memakainya. Kebingungan diatasi dengan petunjuk di form (`FIX-HRD-008-05`). | Tidak ada. `FIX-HRD-008-05` berlaku untuk kedua pilihan |

---

## 8. Catatan Pola

1. **Satu fungsi, dua pintu menu.** Amandemen navigasi Paket A (`03-frontend-architecture.md`
   bagian 34.1) menetapkan "satu fungsi hanya boleh mempunyai satu primary navigation owner".
   Desain Menu 05 (16-11-2026) dibuat empat hari sebelum Paket A (20-11-2026). Paket A lalu
   menandai Menu 05 sebagai `UNCHANGED / REUSED` (bagian 34.2) tanpa memeriksa ulang tumpang
   tindih Struktur Organisasi dan Unit Organisasi.
2. **Satu halaman, dua hak akses.** Mode Hierarki meminjam endpoint milik controller lain.
   Halaman yang mencampur hak akses mudah gagal sebagian untuk peran tertentu. Pemisahan menurut
   Opsi A membuat setiap halaman dijaga satu hak akses saja.
3. **Tautan antarlayar lewat parameter alamat yang tidak dibaca layar tujuan.** Ini terjadi
   pada `?search=` di T1. Sebagai pembanding, tautan Penempatan Awal
   (`initial-placement-batch-view.jsx:44-46`) memakai `?type=`, dan parameter itu memang dibaca
   (`use-master-data-organization.jsx:315`). Pencegahannya: setiap tautan berparameter diuji
   sampai ke layar tujuannya.
4. **Form tidak mengikuti perubahan aturan server.** Ini terjadi pada T2. Saat backend mengambil
   alih penghitungan suatu nilai, isian di form harus ikut dicabut pada task yang sama.

---

## 9. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| --- | --- | --- |
| 2026-11-24 | Issue dibuat dari masukan analis (3 kalimat) dan pilihan Opsi A pemilik: 2 butir pelapor, 5 temuan tambahan, 2 keputusan pemilik | `diagnose-module-issue` |
| 2026-11-24 | Pemilik menyetujui seluruh rencana; K-01 dan K-02 memakai rekomendasi (a). Status → `DALAM_PERBAIKAN`. Keputusan `HRD-DEC-257` s.d. `HRD-DEC-259` (dinomori ulang dari `HRD-DEC-248` s.d. `250` saat merge `QuilvianIntegrationBackEnd` 27-11-2026) | `manage-module-blueprint` |
| 2026-11-24 | Seluruh perbaikan yang disetujui berstatus ✅ pada register plan repair (8/8). Status → `SELESAI`. Uji runtime dengan backend dan akun nyata masih tertunda; backend lokal perlu di-restart | pengerjaan task |
