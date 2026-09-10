# Roadmap Delivery Frontend — Sub-modul `dokter-rawat-inap` (Rawat Inap)

## Metadata

```yaml
module_id: rawat-inap
module_name: InPatientManagement
roadmap_revision: 3
blueprint_id: RWI-BP-001
status: APPROVED
approval_gate: BLUEPRINT_APPROVED
blueprint_shape: COMPOSITE
submodule: dokter-rawat-inap
blueprint_root: docs/module-blueprints/rawat-inap/dokter-rawat-inap/
owners:
  - "Product/Domain: Muhammad Hamzah (RWI-DEC-061)"
  - "Frontend authority: UI/komposisi mengikuti skema-tampilan-dokter-rawat-inap.md; rincian yang tidak dikunci mengikuti token dan pola repository"
approved_by:
  - "Muhammad Hamzah — Product/Domain owner (RWI-DEC-061), approval desain 2026-09-03"
approved_at: "2026-09-03"
revision_authority: "Instruksi pengguna 2026-09-08: rencanakan ulang delivery sub-modul dokter-rawat-inap. Sebelumnya instruksi 2026-09-07: perbarui rules UI FE-RWI-042–050 dan pertahankan menu Dokter → Rawat Inap. Approval desain 2026-09-03 tetap historis"
revision_scope: "Revision 3 menyegarkan masukan yang bergerak, lalu menautkan ketiga task 🟡 ke task backend penutupnya BE-RWI-066 s.d. BE-RWI-068. Nol ID frontend baru dibuat, nol status task diubah, dan bukan approval implementasi maupun rilis"
ui_contract: "skema-tampilan-dokter-rawat-inap.md — UI CONTRACT / VISUAL IMPLEMENTATION CONTRACT"
source_sha:
  backend: "93b3227c431401d8f586dec4e1fb25fbf41766e3"
  frontend: "863f24b0d1617069310c04e5770b47fd1b518b5b"
focused_review_source_sha:
  backend: "350361c7c489e8d3e861908fcc8410ea46bbdabf"
  frontend: "eb505a9ea20d99505a69ff0e6ea428ec9a551dc5"
focused_review_date: "2026-09-07"
replan_source_sha:
  backend: "c82e69f327b01702b6bef595c685b10086ab7bd9"
  frontend: "e194509dc695aaa43264eab3ca7065762b14d2ee"
replan_date: "2026-09-08"
contract_versions: "0.3.0"
input_revisions:
  03-frontend-architecture.md: 0.3
  02-module-map.md: 1
  04-prd-to-mvp.md: 0.3
  skema-tampilan-dokter-rawat-inap.md: "Tidak mencantumkan revision; snapshot dikunci SHA-256"
input_hashes:
  03-frontend-architecture.md: "3e8c04ed74e117d629c678d71f6a63d6e87c69f9fc32fad30d9bbb7b11c4a5a8"
  02-module-map.md: "94e54fd45ba09bd61eb9c218e7d7b424725c3a45b6dd76f16f4a51a98c6f7c85"
  04-prd-to-mvp.md: "a0d5cc0c998fea5d7c23c587eeec1718e456320c6191eb5ffb752e0f3d79f9cb"
  skema-tampilan-dokter-rawat-inap.md: "6a5fcaca9789e6d6d209e52dd84a11587aae71bd740a98aeb261eda1fdc1c2b0"
artifact_hashes:
  contracts/api-contract.md: "bbfa035a6607710f1b2bf30f50b7d8899adcc4b214b28734bc04dba19124bbc3"
  contracts/permission-audit-matrix.md: "7790bbc230e3a39bdfda93a0862cd81004bb035e0614077a48710cc9f99db5b2"
  contracts/integration-contract.md: "b53f73fc6fc40cd6ed8a265564b9c4f37572aa60396dfc43415fe9042e6e2c5b"
  contracts/state-transition-matrix.md: "024c330d0ccf5acf4a94ec5c87e7cde6c92626f8b8fdcd0a86dc086aa8a14802"
  contracts/validation-matrix.md: "cf8033eb2634ef63441d2c157546794dc6c0e716f913928d9ff349ef625ccb57"
task_id_series: "FE-RWI-042 s.d. FE-RWI-050 — deret bersama seluruh modul, dilanjutkan dari FE-RWI-041. **Revision 3 tidak menambah satu pun ID frontend baru**; ketiga task 🟡 ditutup pada ID-nya sendiri"
```

---

## 0. Peringatan yang menentukan seluruh roadmap ini

> **Ruang kerja dokter sudah ada di source, dan memakai kontrak yang salah.** Berbeda dari
> `episode-rawat-inap` yang dibangun dari nol, sub-modul ini **memperbaiki layar yang sudah
> ter-commit**. Statusnya `Conflict` pada `DOK-TRC-FE-01`, dan ia **menahan rilis apa pun** —
> bukan menahan pengembangan.
>
> Perbaikannya menyasar sumber data, pengikatan episode, komposisi layar, dan lifecycle dokumen.
> Reuse dinilai per komponen pada §1.2 dan setiap task. Tab existing hanya boleh diadaptasi bila
> bagian presentasinya netral terhadap antrean; `ClinicalPatientCard` tidak dipakai as-is.

> **Akibat yang harus dinyatakan terus terang.** Dalam bentuk sekarang, layar itu dapat menampilkan
> pasien **rawat jalan** dengan label "Rawat Inap", lalu mengirim aksi antrean terhadap mereka.
> `FE-RWI-042` dan `FE-RWI-043` ada untuk menutup itu, dan keduanya berada paling depan.

> **Keputusan operasional 2026-09-07:** menu **Dokter → Rawat Inap tetap ada**, berdampingan
> dengan Rawat Jalan. Menu membuka daftar pasien rawat inap dokter dari census/episode, lalu
> pilihan pasien membuka satu Physician Workspace. Census dan detail episode tetap menjadi
> alternate entry point. Keputusan ini menggantikan instruksi pencabutan menu pada revision 1.

> **UI CONTRACT:** [skema-tampilan-dokter-rawat-inap.md](../skema-tampilan-dokter-rawat-inap.md)
> menjadi sumber utama layout, komposisi, state visual, dan responsive bagi **FE-RWI-042–050**.
> Business/clinical rule, ownership, API, validasi, dan authority tetap mengikuti lima kontrak
> `0.3.0` serta approved blueprint. Contoh gambar bukan izin membuat status atau kewenangan baru.

---

## 1. Batas kewenangan dokumen ini

| Mengikat | `DEV_DISCRETION` |
| --- | --- |
| Menu Dokter → Rawat Inap dan alur daftar pasien → episode → workspace; alternate entry Census/detail episode | Detail breadcrumb yang tidak mengubah alur atau batas klik |
| Sumber data episode, Patient Safety Context, urutan komponen, enam tab, layout per task | Jarak dan dekorasi yang tidak ditentukan skema, mengikuti design token existing |
| Resource–Action, authority per pasien/dokumen, hide/disable, lifecycle per domain | Kalimat penjelasan dengan makna yang sama |
| Makna loading, empty, error, retry, read-only, permission, episode closed, request in progress | Skeleton dan ikon dengan semantik yang jelas |
| Timeline, segmented navigation, pemisahan domain, responsive dan acceptance visual | Rincian CSS/nama file sesuai konvensi source; bukan mengganti komposisi atau component library |

### 1.1 Rules UI global dan alur operasional

1. Dokter membuka **Dokter → Rawat Inap**; daftar memakai census pasien rawat inap yang disaring
   sesuai dokter yang masuk. Hilangkan filter tanggal hari ini warisan antrean. Pasien yang sudah
   dirawat beberapa hari tetap muncul. Menu bukan bukti authority atas semua pasien.
2. Dokter memilih pasien berdasarkan **episode**. Kunci konteks adalah pasien, kunjungan, dan
   episode yang cocok, bukan `QueueId`/`queueCode`. Route episode mengikuti pola App Router
   existing; URL lama `…/doctor-inpatient` menjadi entry daftar, bukan workspace tanpa episode.
3. Workspace hanya memuat **satu pasien + satu episode**. Konteks harus valid sebelum aksi klinis
   aktif. Saat berpindah pasien/episode, response lama tidak boleh mengisi workspace baru dan
   isian pasien lama tidak boleh terkirim ke episode lain.
4. Urutan wajib: **Patient Safety Context → Clinical Tab Navigation → Active Clinical Content**.
   Composition: `ClinicalPageHeader → InpatientEpisodeHeader → ClinicalContextBar →
   ClinicalSafetyAlert → PhysicianAuthorityIndicator → ClinicalTabNav → TabContent`.
   Tiga komponen konteks/alert/authority dikomposisikan di dalam header episode sesuai skema §20.
5. Context minimal: **nama pasien, No. RM, episode, lokasi/kamar/bed, hari rawat, DPJP, diagnosis
   kerja, alergi, authority pengguna**. Identitas dan alergi lebih dominan daripada dekorasi atau
   metadata administratif. Context tetap terlihat di atas tab, termasuk saat scroll/edit/modal;
   pada layar kecil ringkasan yang menetap harus tetap memuat informasi kritis yang terbaca.
6. Tab utama persis: **Kajian Medis; Catatan Perkembangan; Catatan Terpadu; Visite; Resep & Tindakan;
   Penunjang**. Screening dipetakan ke Kajian Medis, SOAP ke Catatan Perkembangan, CPPT ke Catatan
   Terpadu; Prescription dan Procedure menjadi dua segmen satu tab. Certificate di luar MVP ini.
7. **Context failure mematikan seluruh write action**, termasuk autosave, submit modal, complete,
   verify, cancel, order, dan addendum. Loading/mismatch context juga tidak boleh mengirim tulisan.
   Allergy error selalu terlihat sebagai **“Riwayat alergi tidak dapat dimuat”** dengan retry;
   tidak boleh dipetakan menjadi “tidak ada alergi”. Tidak mengarang larangan klinis baru hanya
   dari allergy error; keputusan eligibility tetap berasal dari kontrak/authority backend.
8. Lepaskan dependency UI `useDoctorQueue`, `useDoctorQueueBoard`, `useInfiniteQueueScroll`,
   `queueCode`, queue call action, skip action, no-show action, queue timer, queue call lock.
   Tidak ada **Panggil, Lewati, Tidak Hadir**. Audit mencakup hook/service turunan, bukan hanya
   import langsung. `useDoctorConsultationWorkspace` yang masih memakai queue contract tidak
   boleh dipakai as-is; pecah/adaptasi hanya bagian netral. Penghapusan berlaku pada jalur
   rawat inap; komponen yang masih dibutuhkan rawat jalan tidak dihapus dari repository.
9. Tidak ada **“Simpan Konsultasi / Finalize Consultation”** global yang menyelesaikan SOAP,
   CPPT, visite, resep, atau tindakan sekaligus. Completion hanya menyelesaikan dokumen terpilih.

| Domain | Lifecycle UI | Batas kontrak yang tetap berlaku |
| --- | --- | --- |
| Kajian Medis | `Draft → Completed` | `InProgress`/`Cancelled` tetap dipetakan bila dikembalikan API; final read-only, koreksi lewat addendum sah |
| Catatan Perkembangan | `Draft → Completed → Correction/Addendum` | Koreksi **tetap `Completed`**, bukan status `Amended` baru; minimal satu bagian SOAP terisi (`VAL-DOK-12`), bukan wajib keempatnya |
| Catatan Terpadu | `Written → Pending Verification → Verified` | `Written` adalah ringkasan alur; status verifikasi API tetap `NotRequired`, `Pending`, `Overdue`, `Verified`; koreksi catatan Verified kembali Pending |
| Visite | `Recorded → Cancelled` | Batal terminal, alasan wajib, riwayat tetap ada; tidak ada Edit |
| Resep | Per prescription | Jenis `Routine`, `Daily`, `Discharge`; status pemenuhan milik Farmasi, read-only |
| Tindakan | Per procedure | `Planned`, `Ordered`, `InProgress`, `Completed`, `Cancelled`; dispatch Billing terpisah dari status klinis |

**Contoh:** pemeriksaan pukul 07.40 dicatat pukul 11.03; timeline memakai 07.40, metadata juga
menampilkan 11.03. Setelah Completed, koreksi oleh dr. Budi tampil sebagai addendum terpisah;
dr. Andi tetap penulis asli. Episode Closed tidak dibuka kembali oleh koreksi tersebut.

### 1.2 Keputusan base component dan bukti reuse

Path berikut relatif terhadap `QuilvianSystemFrontendDev`, pada SHA review di metadata.
Klasifikasi di setiap task wajib membedakan **EXISTING BASE COMPONENT**, **NEW BASE COMPONENT**,
**DOMAIN COMPONENT**, dan **COMPONENT TO REMOVE / NOT REUSE**. Extend/adapter tidak berarti
memasukkan aturan DPJP, kebijakan verifikasi, status episode, API, atau billing ke base component.

| Existing base component | Keputusan dan composition | Bukti source |
| --- | --- | --- |
| `ClinicalPageHeader` | Reuse kepala workspace/entry | `src/components/ui/doctor-clinical-base/index.js` export `ClinicalPageHeader` |
| `ClinicalSummaryBar` | Selective untuk ringkasan jumlah pasien/monitoring bila datanya tersedia; bukan metrik antrean | Folder yang sama, `index.js` export `ClinicalSummaryBar` |
| `ClinicalStatusBadge` | Reuse; label/tone status dikirim adapter domain, tidak memakai makna waiting/called/skipped sebagai status rawat inap | Folder yang sama, `index.js` export `ClinicalStatusBadge` |
| `ClinicalContextBar` | Adapter pada `InpatientEpisodeHeader` memetakan pasien sebagai identitas utama, info episode, DPJP, dan slot alert/authority | Folder yang sama, `ClinicalContextBar.jsx`, props `primaryValue`, `infoItems`, `children` |
| `ClinicalTabNav` ✅ | Reuse enam tab horizontal, scroll saat perlu; extend generik relasi tab/panel dan keyboard bila belum tersedia | Folder yang sama, `ClinicalTabNav.jsx`, `tabs`, `activeTab`, `onTabChange`; source sudah punya `role=tablist/tab`. **Diperluas 7 September 2026**: `aria-controls`, roving `tabIndex`, navigasi `ArrowLeft`/`ArrowRight`/`Home`/`End`, dan pembangun id tab/panel; props lama tidak berubah — [FE-RWI-043](../task/report/frontend/FE-RWI-043.md) |
| `ClinicalSectionPanel` | Reuse pembungkus wilayah klinis; bukan seluruh workspace dalam satu panel | Folder yang sama, `index.js` export `ClinicalSectionPanel` |
| `ClinicalDataTable` | Reuse tabel order/tindakan; adapter menyediakan `columns`, `rows`, `rowKeyFn` berbasis id domain | Folder yang sama, `ClinicalDataTable.jsx`; empty bawaan hanya menguji `rows.length`, bukan status request |
| `ClinicalEmptyState` | Reuse pesan empty melalui `title`, `description`, `children`; loading/error/retry diatur boundary tersendiri | Folder yang sama, `ClinicalEmptyState.jsx`; source bukan pengelola request/state async |
| `ClinicalPatientCard` | **NOT REUSE AS-IS**; gunakan tabel pasien episode dan `InpatientEpisodeHeader` | Folder yang sama, `ClinicalPatientCard.jsx`, props/render `queueCode` dan tone antrean |

Base baru berikut **direncanakan**, belum exported pada `doctor-clinical-base/index.js` saat
review. Semua tetap generik di `src/components/ui/doctor-clinical-base/`. Task pemilik membuat
sekali; task konsumen menggunakannya tanpa membuat salinan.

| New base component | Task pemilik | Consumer | Tanggung jawab generik |
| --- | --- | --- | --- |
| `ClinicalStateBoundary` ✅ | FE-RWI-042 | FE-RWI-043–050 | Rendering loading/empty/error/retry/read-only/denied berdasarkan props; tidak fetch API. **Sudah dibuat dan diekspor 7 September 2026** pada `src/components/ui/doctor-clinical-base/ClinicalStateBoundary.jsx` — [FE-RWI-042](../task/report/frontend/FE-RWI-042.md) |
| `ClinicalSafetyAlert` ✅ | FE-RWI-043 | FE-RWI-044–049 | Pesan safety, error alergi, peringatan non-final/visite dekat; tidak menghitung risiko klinis. **Sudah dibuat dan diekspor 7 September 2026** pada `src/components/ui/doctor-clinical-base/ClinicalSafetyAlert.jsx` — [FE-RWI-043](../task/report/frontend/FE-RWI-043.md) |
| `ClinicalActionGuard` ✅ | FE-RWI-043 | FE-RWI-044–049 | Hide/disable/penjelasan dari hasil authority domain; bukan mesin permission backend. **Sudah dibuat dan diekspor 7 September 2026** pada `src/components/ui/doctor-clinical-base/ClinicalActionGuard.jsx`; mode disable memakai `<fieldset disabled>` sehingga kontrol tulis benar-benar mati — [FE-RWI-043](../task/report/frontend/FE-RWI-043.md) |
| `ClinicalValidationSummary` ✅ | FE-RWI-044 | Form domain lain bila perlu | Daftar validasi dan tautan fokus ke field; aturan wajib tetap dari kontrak. **Sudah dibuat dan diekspor 8 September 2026** pada `src/components/ui/doctor-clinical-base/ClinicalValidationSummary.jsx` — [FE-RWI-044](../task/report/frontend/FE-RWI-044.md) |
| `ClinicalCompletionBar` ✅ | FE-RWI-044 | FE-RWI-045 | Peringatan penguncian, Simpan Draft/Selesaikan satu dokumen, pending; tidak memfinalkan domain lain. **Sudah dibuat dan diekspor 8 September 2026** pada `src/components/ui/doctor-clinical-base/ClinicalCompletionBar.jsx` — [FE-RWI-044](../task/report/frontend/FE-RWI-044.md) |
| `ClinicalTimeline`, `ClinicalTimelineItem` ✅ | FE-RWI-045 | FE-RWI-046, FE-RWI-047 | Daftar/select item, slot waktu, metadata, isi/detail, audit; urutan dari adapter domain. **Sudah dibuat dan diekspor 8 September 2026** pada `src/components/ui/doctor-clinical-base/ClinicalTimeline.jsx` dan `ClinicalTimelineItem.jsx`; item yang dapat dipilih dirender sebagai tombol sehingga fokus papan ketik ikut terpakai — [FE-RWI-045](../task/report/frontend/FE-RWI-045.md) |
| `ClinicalDocumentMeta` ✅ | FE-RWI-045 | FE-RWI-046, FE-RWI-047; detail kajian/tindakan bila perlu | Label penulis, profesi, waktu klinis, waktu catat, status; tidak mengganti penulis. **Sudah dibuat dan diekspor 8 September 2026** pada `src/components/ui/doctor-clinical-base/ClinicalDocumentMeta.jsx`; waktu klinis dan waktu catat menempati baris sendiri-sendiri — [FE-RWI-045](../task/report/frontend/FE-RWI-045.md) |
| `ClinicalAuditBadge` ✅ | FE-RWI-045 | FE-RWI-046, FE-RWI-047 | Penanda koreksi/verifikasi/terlambat/batal, teks plus visual. **Sudah dibuat dan diekspor 8 September 2026** pada `src/components/ui/doctor-clinical-base/ClinicalAuditBadge.jsx`; ikon selalu ditemani teks sehingga warna bukan satu-satunya pembeda — [FE-RWI-045](../task/report/frontend/FE-RWI-045.md) |
| `ClinicalSegmentedNav` ✅ | FE-RWI-048 | FE-RWI-049 hanya bila kelak memakai segmen | Navigasi internal generik; FE-RWI-049 memakai dua section pada revision ini. **Sudah dibuat dan diekspor 8 September 2026** pada `src/components/ui/doctor-clinical-base/ClinicalSegmentedNav.jsx`; dirender sebagai `role="radiogroup"`, **bukan** `tablist` kedua, supaya relasi tab/panel shell tidak terbaca ganda. FE-RWI-049 memang tidak memakainya, sehingga nol dependency baru antar-cabang dibuat — [FE-RWI-048](../task/report/frontend/FE-RWI-048.md) |

**Urutan pembuatan komponen bukan dependency backend baru.** Kolom Dependency existing tetap.
Cabang boleh menyiapkan adapter/domain masing-masing secara paralel, tetapi integrasi consumer
menunggu base pemilik: FE-RWI-045 memakai completion FE-RWI-044; FE-RWI-046/047 memakai timeline
FE-RWI-045. Tidak perlu menunggu seluruh fitur task pemilik untuk meninjau base yang netral.

Struktur konseptual domain yang mengikat separation of concern:

```text
src/components/view/health-services/inpatient-management/physician-workspace/
├── physician-workspace-client.jsx    # orkestrasi konteks, request, state, adapter
├── physician-workspace-view.jsx      # composition shell dan tab aktif
├── components/                      # header episode, alergi, authority, tab mapping
├── tabs/
│   ├── assessment/
│   ├── progress-note/
│   ├── integrated-note/
│   ├── physician-visit/
│   ├── medication-procedure/
│   └── supporting-service/
└── modals/                          # complete, correction, record/cancel visit
```

Nama file boleh menyesuaikan naming convention source, tetapi tidak boleh menjadi satu giant
component. Hook/service/Redux tetap di folder feature yang berlaku. Monitoring tetap di layar
existing `inpatient-monitoring-view.jsx`, bukan dipindah menjadi workspace kedua.

### 1.3 State Handling dan Permission Behaviour global

Setiap task mewarisi tabel ini **dan** rincian state lokalnya. EMPTY ≠ ERROR;
NOT REQUIRED ≠ COMPLETED; BELUM FINAL ≠ FINAL. Status UI bukan penambahan enum backend.

| State | Perilaku wajib |
| --- | --- |
| `LOADING` | Skeleton konteks/baris; jangan menampilkan sukses kosong saat request belum selesai; write menunggu konteks dan authority valid |
| `EMPTY` | Hanya setelah baca berhasil; pesan spesifik domain, misalnya “Belum ada visite tercatat”; empty child tidak mematikan fitur independen |
| `ERROR` | Pesan terlihat pada wilayah yang gagal; context error mematikan semua write, error child ditangani lokal; isi draft tetap ada |
| `RETRY` | Coba Lagi mengulang baca wilayah yang gagal; retry mutation memakai payload/key yang sama sesuai kontrak dan merekonsiliasi hasil tidak pasti, bukan membuat record baru |
| `READ ONLY` | Informasi terbaca bagi pemegang Read; kontrol edit diganti detail; alasan jelas; final tidak dapat diedit langsung |
| `PERMISSION DENIED` | Tanpa Read, tampilkan gate dan jangan bocorkan data. Aksi yang tidak dimiliki disembunyikan, terutama Koreksi/Verifikasi. Authority pasien yang tidak sah menonaktifkan area tulis dengan penjelasan DPJP, tanpa menampilkan aksi yang memang tidak dimiliki |
| `EPISODE CLOSED` | Banner “Episode telah ditutup”; dokumen/order/visite baru tidak boleh dibuat. Addendum dokumen terkunci tetap tersedia hanya jika context valid dan authority koreksi sah; tidak membuka episode/bed. `Cancelled`/`Draft` episode mengikuti `VAL-DOK-02/03` |
| `DOUBLE SUBMIT / REQUEST IN PROGRESS` | Tombol pending/nonaktif per aksi dan guard handler; tidak ada autosave/complete yang berlomba. Visite/resep/tindakan memakai idempotency sesuai kontrak; timeout tidak diasumsikan gagal tersimpan |

Refresh konteks/authority ketika fokus kembali atau respons `403`/`409`/`422` menunjukkan data
berubah. `403` menjelaskan kewenangan; `409` final mengarahkan ke koreksi atau event sudah batal;
`422` menjelaskan episode tidak layak. Read-only episode bukan pengganti authority koreksi.
Resource–Action dari `api-contract.md` serta authority pasien/dokumen dari backend sama-sama wajib;
label peran saja tidak cukup. Addendum pengganti mengikuti `RWI-DEC-088`, tanpa form menerbitkan
penetapan berhalangan di workspace. Konflik peran yang belum terjawab dicatat pada §6.

### 1.4 Responsive Behaviour dan bukti visual

| Ukuran | Acceptance minimum |
| --- | --- |
| Desktop ≥1200 px | Full context, tab horizontal; kajian 70% form / 30% referensi; SOAP timeline kiri dan editor/detail kanan |
| Tablet 768–1199 px | Context dua kolom, tab boleh horizontal scroll; SOAP 35% timeline / 65% editor bila muat, form/aksi tetap terbaca |
| Mobile <768 px | Satu kolom; context paling atas dan informasi kritis tetap terbaca. Kajian: form → referensi → completion. Timeline: pilih catatan → detail/editor di bawah; modal dapat discroll dan tombol terjangkau |

Tabel dalam kolom konten boleh scroll horizontal lokal dengan header/label yang tetap terbaca;
tidak boleh menyebabkan overflow seluruh halaman atau menyembunyikan status/aksi kritis.
Gunakan token existing, status dengan teks (bukan warna saja), fokus keyboard terlihat, dan
relasi tab/panel yang dapat diakses. Bukti visual tiap task minimal desktop 1440 px, tablet
1024 px, mobile 390 px, menyebut tinggi viewport, state, peran, dan episode uji samaran.
Screenshot dan pemeriksaan interaksi membuktikan hasil; skema saja bukan bukti implementasi.

### 1.5 Konflik revision 1 dan keputusan revision 2

| Konflik/gap lama | Sumber pembanding | Penyelesaian roadmap |
| --- | --- | --- |
| §0/§1/§5 menyerahkan susunan visual dan bentuk tab sebagai DEV_DISCRETION | Skema §3–20, instruksi pengguna | Layout, composition, base/domain, responsive dikunci; detail dekoratif mengikuti token |
| FE-RWI-042 mencabut menu, milestone menghilangkan menu, §5 menyatakan nol entry menu | Skema §1/§2/§21 dan keputusan operasional pengguna 2026-09-07 | Dokter → Rawat Inap tetap tersedia; daftar pasien episode; Census/detail menjadi alternate entry |
| FE-RWI-043 Reuse memasukkan kartu pasien tanpa batas; reuse task lain terlalu umum | Skema §4/§5/§15/§20; source ClinicalPatientCard | Kartu antrean dilarang as-is; reuse/extend/new/domain/remove disebut tiap task |
| Layout form, timeline, internal segmen dan modal belum eksplisit | Skema §6–13 | Kajian 70/30; SOAP dua kolom; CPPT lintas profesi; event visite; Resep/Tindakan dua segmen; Lab/Rad dua section; monitoring existing |
| State task hanya empat keadaan atau “tiga keadaan kosong” | Skema §17/§18 | Delapan state async per task; not-required dan error bukan empty sukses |
| Roadmap belum melarang global finalize yang ada di source | Skema §16; source doctor-inpatient-view.jsx bagian “Simpan Konsultasi / Visite” | Completion satu dokumen/domain; tanpa coupling penyelesaian semua tab |
| Tidak ada Visual Acceptance Criteria per task/global gate | Skema §22 | Bukti visual per task dan 20 gerbang §4.1 |
| Contoh verifikator pada skema §9 dapat terbaca sebagai penulis memverifikasi sendiri | `state-transition-matrix.md` §3 dan `permission-audit-matrix.md` §3 | Kontrak menang: DPJP aktif **bukan penulis asli**; contoh acceptance memakai Ns. Sari/dr. Andi |

Baseline `source_sha` approval tetap dipertahankan; SHA review hanya untuk bukti terarah, bukan
klaim audit ulang seluruh backend. Hash peta modul disegarkan dari `29c761ee…` ke `7e955dbb…`
(revision tertulis tetap 1); §3.3 masih memerintahkan pencabutan menu dan **superseded khusus
aturan entry dokter** oleh instruksi pengguna ini. `IA-INP-05` dan ownership data tetap berlaku.
Arsitektur §2/§7 dan PRD §20.1 masih membawa aturan menu/rupa lama; catatan ini menjadi amendment
terbatas bagi implementer. Dokumen hulu, skema, kontrak, manifest, dan traceability tidak disunting.
Sinkronisasi metadata roadmap revision 1 pada manifest/traceability merupakan pekerjaan dokumen
lanjutan di luar target tulis ini; identifier requirement dan task di sana tetap valid.

---

### 1.6 Gelombang penutup — apa yang menahan ketiga task 🟡, dan siapa yang mencabutnya

Ditambahkan revision 3, 8 September 2026.

Kesembilan task frontend sudah dikerjakan. Enam ✅ selesai, dan **tiga berhenti di 🟡 `SEBAGIAN`**.
Ketiganya berhenti karena alasan yang sama bentuknya: **layar sudah dibuat, tetapi backend tidak
mengirimkan keterangan yang harus ditampilkan.** Tidak satu pun berhenti karena source frontend-nya
kurang.

Bedakan dua hal yang mudah tertukar. Sebuah layar yang **belum dibuat** adalah kekurangan
pekerjaan, dan itu diselesaikan dengan mengerjakannya. Sebuah layar yang **sudah dibuat tetapi
tidak punya data untuk ditampilkan** adalah kekurangan pada sumber datanya, dan mengerjakannya
berulang kali di sisi frontend tidak akan pernah menutupnya. Ketiga task di bawah adalah jenis
kedua.

| Task 🟡 | Yang belum terbukti | Kenapa layar tidak bisa menutupnya sendiri | Yang menutupnya |
| --- | --- | --- | --- |
| `FE-RWI-046` | Kriteria 2 separuh — **nama verifikator tidak dapat ditampilkan** | Balasan baca catatan terpadu tidak memuat satu pun kolom verifikasi. Menebaknya berarti layar mengarang "sudah diverifikasi" dari ketiadaan bukti, dan itu justru kesalahan yang paling berbahaya pada rekam medis | **`BE-RWI-066`** |
| `FE-RWI-050` | Kriteria 4 separuh — **kolom Penulis kosong** | Butir daftar pantau hanya membawa nomor pengguna. Menampilkan nomor itu apa adanya tidak menolong siapa pun, dan menerjemahkannya sendiri di layar berarti frontend menyimpan salinan daftar nama pegawai | **`BE-RWI-067`** |
| `FE-RWI-050` | Kriteria 5 belum — **urutan daftar belum ditetapkan** | Urutan daftar di dalam `FE-INP-09` ditetapkan tingkat modul, bukan oleh sub-modul. Memutuskannya sendiri berarti melanggar batas yang justru dibuat supaya tiga sub-modul tidak menyusun satu layar bersama sendiri-sendiri | **Keputusan pemilik `02-module-map.md`**, bukan task |
| `FE-RWI-044` | 1 elemen layout — **tombol Tambah Diagnosis** | Diagnosis terstruktur wajib menyebut nomor konsultasi, sehingga hanya dapat lahir dari catatan dokter. Layar sudah menampilkan daftar masalahnya hanya-baca beserta keterangan ke mana penambahannya menempel | **`BE-RWI-068`** ⛔, yang sendirinya menunggu kontrak |

**Ketiga task ditutup pada ID-nya sendiri, dan revision ini tidak membuat ID frontend baru.**
Alasannya bukan penghematan nomor. Acceptance criteria yang belum terbukti masih tercatat pada
kartu task pemiliknya; memindahkannya ke ID baru berarti sesudah itu tidak ada lagi satu tempat pun
yang dapat menjawab "apakah `FE-RWI-046` sudah selesai". Register status akan memperlihatkan enam ✅
dan tiga ID baru, sementara tiga task lamanya menggantung 🟡 selamanya tanpa pernah ada yang
menutupnya.

**Urutan pengerjaannya sesudah task backend selesai:**

```text
BE-RWI-066 selesai ──> jalankan ulang builder frontend pada FE-RWI-046
                       lalu FE-RWI-050 ikut terbantu untuk kolom statusnya

BE-RWI-067 selesai ──> jalankan ulang builder frontend pada FE-RWI-050
                       kriteria 4 tertutup; kriteria 5 tetap menunggu keputusan urutan

BE-RWI-068 selesai ──> jalankan ulang builder frontend pada FE-RWI-044
                       (BE-RWI-068 sendiri masih ⛔ menunggu kontrak API)
```

> **Satu hal yang tidak berubah karena revision ini.** Gelombang `DOK-MVP-FE` **tetap belum boleh
> dinyatakan selesai**, dan sub-modul ini tetap belum boleh dirilis. Revision 3 hanya menuliskan
> dengan jelas apa yang menahannya dan siapa yang memegang kuncinya — ia tidak mencabut satu pun
> gerbang pada bagian 4 dan tidak menaikkan status satu pun task.

---

## 2. Slice dan milestone

Seluruh task frontend berada pada gelombang **`DOK-MVP-FE`**, dan gelombang itu **wajib selesai
sebelum rilis apa pun**. Urutan di dalamnya mengikuti kesiapan endpoint backend.

| Urutan | Task | Bergantung pada backend | Yang dapat diverifikasi bisnis |
| ---: | --- | --- | --- |
| 1 | `FE-RWI-042` ✅ | — | Menu Dokter → Rawat Inap membuka daftar pasien episode; Census/detail tetap dapat dipakai |
| 2 | `FE-RWI-043` ✅ | `BE-RWI-044` | Dokter membuka pasiennya sendiri, bukan antrean poliklinik |
| 3 | `FE-RWI-044` 🟡 | `BE-RWI-045` | Kajian medis awal dapat ditulis dan diselesaikan |
| 4 | `FE-RWI-045` ✅ | `BE-RWI-046`, `BE-RWI-047` | Catatan harian ditulis, diurut waktu pemeriksaan, dan dikoreksi |
| 5 | `FE-RWI-047` ✅ | `BE-RWI-048`, `BE-RWI-049` | Visite dicatat, dibatalkan, dan riwayatnya terbaca |
| 6 | `FE-RWI-048` ✅ | `BE-RWI-050`, `BE-RWI-051` | Resep dan tindakan dikerjakan dari satu layar |
| 7 | `FE-RWI-049` ✅ | `BE-RWI-052` | Pemeriksaan laboratorium dan radiologi dipesan, hasilnya dibaca |
| 8 | `FE-RWI-046` 🟡 | `BE-RWI-053` | Catatan terpadu dibaca dan diverifikasi DPJP |
| 9 | `FE-RWI-050` 🟡 | `BE-RWI-053` | Supervisor melihat verifikasi yang tertunggak |

### Urutan dependency

Grafik di bawah adalah **tampilan lain dari kolom `Dependency` pada bagian 3**, bukan sumber
kebenaran baru. Bila keduanya berbeda, kolom `Dependency` pada tabel task yang berlaku.
Status mengikuti baris `Status` pada masing-masing task di bagian yang sama.

```text
FE-RWI-042 (menu Dokter → Rawat Inap; daftar pasien episode)   ✅ SELESAI 2026-09-07
   └── FE-RWI-043 (ruang kerja dan konteks pasien)             ✅ SELESAI 2026-09-07
          + BE-RWI-044
          ├── FE-RWI-044 (kajian medis awal)                  🟡 SEBAGIAN 2026-09-08
          │      + BE-RWI-045
          ├── FE-RWI-045 (catatan perkembangan dan koreksi)   ✅ SELESAI 2026-09-08
          │      + BE-RWI-046, BE-RWI-047
          ├── FE-RWI-047 (pencatatan dan pembatalan visite)   ✅ SELESAI 2026-09-08
          │      + BE-RWI-048, BE-RWI-049
          ├── FE-RWI-048 (resep dan tindakan)                ✅ SELESAI 2026-09-08
          │      + BE-RWI-050, BE-RWI-051
          ├── FE-RWI-049 (laboratorium dan radiologi)         ✅ SELESAI 2026-09-08
          │      + BE-RWI-052
          └── FE-RWI-046 (catatan terpadu dan verifikasi DPJP) 🟡 SEBAGIAN 2026-09-08 ← nama verifikator tidak ada di kontrak baca
                 + BE-RWI-053
                 └── FE-RWI-050 (daftar pantau verifikasi)   🟡 SEBAGIAN 2026-09-08 ← urutan daftar menunggu peta modul
                        + BE-RWI-053
```

Garis induk menunjukkan **dependency frontend**. Tanda `+` pada baris tepat di bawah task
menunjukkan **dependency backend tambahan**; seluruh prasyarat itu tetap berlaku. Contoh:
`FE-RWI-045` bergantung pada `FE-RWI-043`, `BE-RWI-046`, dan `BE-RWI-047` sekaligus.
Status dan bukti penyelesaian backend mengikuti [roadmap backend](backend-roadmap.md#4-task).

**Cabang setelah `FE-RWI-043` tidak saling menunggu.** `FE-RWI-044`, `FE-RWI-045`,
`FE-RWI-047`, `FE-RWI-048`, `FE-RWI-049`, dan `FE-RWI-046` dapat dikerjakan paralel setelah
prasyarat masing-masing terpenuhi. Urutan pada tabel milestone tidak menambah dependency
antar-cabang. `FE-RWI-050` tetap menunggu `FE-RWI-046` dan `BE-RWI-053`. Handoff base component
lintas cabang mengikuti §1.2 sebelum integrasi; ini tidak menghapus atau menambah dependency backend.

---

## 3. Task

### ✅ `FE-RWI-042` — Operational Entry Point Dokter Rawat Inap

| Field | Isi |
| --- | --- |
| **Status** | ✅ `SELESAI` **7 September 2026.** Keenam acceptance criteria terpetakan ke source yang ada. Validasi nyata: `npm run lint:errors` 0 error; `npm run test:unit` 442/442 lulus, 0 gagal; `npm run build` berhasil dan kedua route (`/health-services/inpatient-management/doctor-inpatient`, `/health-services/inpatient-management/episodes/[id]/physician`) muncul pada `.next/app-path-routes-manifest.json`; 9 skenario peramban `tests/e2e/inpatient-physician-entry.spec.mjs` lulus di Edge (menu, dua entry alternatif, episode selesai, memuat, ditolak, gagal, tiga viewport); 1 skenario interaktif filter/pencarian/reset/paginasi lulus. Gerbang §4.1 butir 1, 2, 18, 20 terbukti untuk task ini. Tidak ada butir DoD yang dikecualikan; isi klinis workspace (Patient Safety Context lengkap dan enam tab) memang scope `FE-RWI-043`. Laporan: [`task/report/frontend/FE-RWI-042.md`](../task/report/frontend/FE-RWI-042.md) |
| **Outcome** | Dokter membuka menu Dokter → Rawat Inap, melihat daftar pasien rawat inapnya, lalu memilih satu episode untuk membuka Physician Workspace; menu Rawat Jalan tetap berdampingan |
| **Trace** | `DOK-TRC-FE-01`; `03-frontend-architecture.md` §0, §2.1, §3.1.1; `02-module-map.md` §3.3; `IA-INP-01`, `IA-INP-05` |
| **Kontrak** | `0.3.0` |
| **Reuse** | Daftar pasien dirawat di backend beserta penyaring dokter; `inpatient-census.service.js` dan pola `inpatient-census-view.jsx` sebagai referensi adapter daftar; menu existing pada `src/utils/menu-sidebar/menu-items.jsx` |
| **UI Contract** | [skema-tampilan-dokter-rawat-inap.md](../skema-tampilan-dokter-rawat-inap.md) §1–5, §15, §17–19, §21–22; keputusan menu pengguna 2026-09-07 dan rules §1 roadmap ini |
| **Layout** | Sidebar Dokter berisi Rawat Jalan dan Rawat Inap. Halaman entry: ClinicalPageHeader → filter pasien rawat inap/dokter sesuai hak baca → tabel pasien episode (nama, No. RM, episode, lokasi/kamar/bed, DPJP, status) → aksi Buka Workspace per baris. Tidak ada panel antrean kiri di workspace |
| **Existing Base Components** | `ClinicalPageHeader`, `ClinicalDataTable`, `ClinicalStatusBadge`, `ClinicalEmptyState`; `ClinicalSummaryBar` selective hanya bila jumlah pasien tersedia, bukan count antrean |
| **Base Components To Extend** | Tidak ada perubahan API base wajib; adapter kolom/`rowKeyFn` tabel memakai id episode. Menu/routing existing diperbaiki dalam scope entry |
| **New Base Components** | Membuat `ClinicalStateBoundary` generik sebagai pemilik pertama (§1.2), untuk loading/empty/error/retry/denied daftar |
| **Domain Components** | `InpatientPhysicianPatientList`, `InpatientPhysicianPatientFilters`, adapter pemilihan episode dan navigasi; reuse penyaring census, tidak membuat mesin daftar antrean baru |
| **Components / Legacy Behaviour To Remove** | Seluruh daftar §1.1 butir 8 dari jalur rawat inap; `ClinicalPatientCard` as-is, filter hari ini, queue row key, Panggil/Lewati/Tidak Hadir. `useDoctorConsultationWorkspace` hanya kandidat ekstraksi bagian netral |
| **State Handling** | `LOADING`: skeleton daftar; `EMPTY`: tidak ada pasien rawat inap sesuai filter dokter; `ERROR`: daftar gagal, bukan kosong; `RETRY`: muat ulang census; `READ ONLY`: entry hanya membaca; `PERMISSION DENIED`: gate daftar tanpa data; `EPISODE CLOSED`: pembukaan riwayat berizin menuju workspace read-only, bukan pasien aktif palsu; `DOUBLE SUBMIT / REQUEST IN PROGRESS`: pemilihan/navigasi tidak menggandakan request atau mengganti episode dengan respons lama |
| **Permission Behaviour** | Menu tersedia bagi pengguna operasional yang berhak; daftar memakai `InpatientCensus : Read`, context memakai izin Read episode dan `DoctorConsultation : Read` saat membuka workspace. Identitas/filter dokter dari akun terautentikasi, bukan queue. Menu tidak memberi authority klinis tambahan; bukti nama permission census pada §6, cocokkan ulang bila SHA implementasi berubah |
| **Responsive Behaviour** | §1.4: desktop tabel penuh; tablet filter dan identitas dua kolom; mobile daftar/kolom bertumpuk dengan nama, RM, episode, lokasi, status dan Buka Workspace tetap terbaca. Patient Safety Context muncul paling atas setelah episode dipilih |
| **Scope** | Pertahankan menu Dokter → Rawat Inap sebagai operational entry point menuju daftar pasien rawat inap dokter; pilih episode sebelum workspace; Census/detail episode menjadi alternate entry point; hentikan ketergantungan antrean dan filter tanggal bawaannya |
| **Dependency** | — |
| **Acceptance Criteria** | 1. Menu **Dokter → Rawat Inap tetap tersedia**, bersama Rawat Jalan. 2. Workspace dapat dicapai melalui menu → daftar pasien → episode, juga melalui Census dan detail episode. 3. Daftar memakai census pasien rawat inap sesuai dokter; pasien rawat jalan tidak muncul. 4. Nol request layanan antrean pada entry/workspace dan dependency turunannya. 5. Tidak ada Panggil/Lewati/Tidak Hadir, timer, lock, atau queueCode. 6. Satu baris episode membuka pasien/episode yang cocok; pasien hari rawat ke-3 tidak hilang karena filter tanggal antrean |
| **Visual Acceptance Criteria** | Menu terlihat bagi peran berhak; judul/kolom jelas menyatakan rawat inap; episode dan lokasi terbaca sebelum memilih; loading, kosong, gagal, denied berbeda; enam tab workspace tidak dibuat sebagai menu sidebar tambahan; desktop/tablet/mobile memenuhi §1.4 |
| **Verification** | Test state komponen dan network membuktikan request census, tanpa queue; scan import dan dependency transitif; fixture rawat jalan versus rawat inap; uji navigasi menu, Census, detail dan batas tiga klik `IA-INP-01`; bukti visual tiga viewport dan state denied/error |
| **Risk / Blocker** | Aturan menu pada arsitektur §2 dan peta modul §3.3 sudah superseded hanya untuk entry dokter; jangan diterapkan kembali. Source daftar/permission harus diverifikasi saat implementasi; tidak mengarang resource baru. Owner: Frontend authority |
| **DoD** | Seluruh Acceptance Criteria, Visual Acceptance Criteria, state/permission dan gate §4.1 yang relevan terbukti; menu tetap ada, nol dependency antrean; laporan tracked menyebut route, source request, bukti tiga entry dan viewport |

---

### ✅ `FE-RWI-043` — Ruang kerja dokter berdiri di atas konteks pasien yang pasti

| Field | Isi |
| --- | --- |
| **Status** | ✅ `SELESAI` **7 September 2026.** Keenam acceptance criteria fungsional dan keenam acceptance visual terpetakan ke source yang ada. Validasi nyata: `npm run lint:errors` 0 error; `npm run test:unit` 458/458 lulus, 0 gagal (16 test baru task ini); `npm run build` beserta `postbuild` berhasil; **22 skenario peramban lulus** di Edge — 13 skenario `FE-RWI-043` (konteks lengkap, navigasi tab klik dan papan ketik, alergi gagal versus kosong, konteks gagal menutup tulisan, kewenangan gagal/ditolak tidak dianggap berwenang, bukan DPJP, DPJP aktif, episode Closed, pergantian episode, lokasi gagal, tiga viewport) ditambah 9 skenario regresi `FE-RWI-042`. Satu cacat keselamatan ditemukan uji dan diperbaiki: penugasan DPJP yang dijawab `403` sempat terbaca sebagai berwenang. Gerbang §4.1 butir 1, 2, 3, 4, 5, 18, 19, 20 terbukti untuk lingkup task ini. Tidak ada butir DoD yang dikecualikan; isi dokumentasi keenam tab memang scope `FE-RWI-044` s.d. `FE-RWI-049`. Laporan: [`task/report/frontend/FE-RWI-043.md`](../task/report/frontend/FE-RWI-043.md) |
| **Outcome** | Sebelum dokter dapat menulis apa pun, layar sudah memastikan pasien, lokasi, penanggung jawab, dan riwayat alerginya benar-benar tampil |
| **Trace** | `FE-DOK-01`; `03-frontend-architecture.md` §3.1; `INV-DOK-01`, `INV-DOK-02` |
| **Kontrak** | `0.3.0` |
| **Reuse** | Katalog existing §1.2 dengan adapter episode; `ClinicalPatientCard` tidak dipakai as-is. Shell existing direkomposisi tanpa hook queue |
| **UI Contract** | [skema-tampilan-dokter-rawat-inap.md](../skema-tampilan-dokter-rawat-inap.md) §2–6, §14–20, §22; seluruh rules §1 roadmap |
| **Layout** | Patient Safety Context + Clinical Tab Navigation + Active Clinical Content. `ClinicalPageHeader → InpatientEpisodeHeader → ClinicalContextBar → ClinicalSafetyAlert → PhysicianAuthorityIndicator → ClinicalTabNav → TabContent`; header berisi nama, No. RM, episode, lokasi/kamar/bed, hari rawat, DPJP, diagnosis kerja, alergi, authority; enam tab sesuai §1.1 |
| **Existing Base Components** | `ClinicalPageHeader`, `ClinicalContextBar`, `ClinicalStatusBadge`, `ClinicalTabNav`, `ClinicalSectionPanel`, `ClinicalEmptyState`; `ClinicalSummaryBar` selective untuk ringkasan episode yang tersedia, tidak wajib pada shell |
| **Base Components To Extend** | `ClinicalContextBar` melalui adapter props/slot `InpatientEpisodeHeader`; `ClinicalTabNav` dilengkapi relasi tab/panel dan navigasi keyboard generik bila belum tersedia. Tidak menyimpan rule episode/DPJP di base |
| **New Base Components** | Membuat `ClinicalSafetyAlert`, `ClinicalActionGuard`; memakai `ClinicalStateBoundary` milik FE-RWI-042. Domain menentukan eligibility, base hanya menampilkan |
| **Domain Components** | `PhysicianWorkspaceClient`, `PhysicianWorkspaceView`, `InpatientEpisodeHeader`, `InpatientAllergyAlert`, `PhysicianAuthorityIndicator`, `PhysicianWorkspaceTabs`, adapter context dan TabContent; folder sesuai §1.2 |
| **Components / Legacy Behaviour To Remove** | §1.1 butir 8 seluruhnya; kartu pasien ber-queueCode, board antrean kiri, `useDoctorConsultationWorkspace` as-is, global Simpan Konsultasi/Visite, tab Certificate dalam MVP |
| **State Handling** | `LOADING`: skeleton safety context dan write disabled; `EMPTY`: episode tidak ditemukan/belum dipilih, tanpa write; `ERROR`: context error global, allergy error alert tersendiri; `RETRY`: ulang sumber yang gagal; `READ ONLY`: dokumen terbaca tanpa edit; `PERMISSION DENIED`: gate Read/authority belum sah; `EPISODE CLOSED`: banner read-only dengan pengecualian addendum sah; `DOUBLE SUBMIT / REQUEST IN PROGRESS`: guard menutup semua jalur tulis saat context invalid/pending, response episode lama diabaikan |
| **Permission Behaviour** | Baca episode/penugasan/alergi/diagnosis sesuai resource API; write harus memenuhi permission aksi, authority pasien dan lifecycle dokumen sekaligus. Context gagal mematikan semua write termasuk koreksi; authority gagal tidak dianggap berwenang. Refresh authority saat fokus kembali; aksi yang tidak dimiliki hidden, area write tanpa authority episode disabled dengan penjelasan |
| **Responsive Behaviour** | §1.4: desktop full context dan tab horizontal; tablet context dua kolom dan tab scroll; mobile satu kolom, context safety tetap di atas/terlihat, bukan tersembunyi dalam tab atau drawer |
| **Scope** | Kepala konteks; penanda alergi; diagnosis kerja; penanda kewenangan; penonaktifan tombol tulis saat konteks gagal; pelepasan aksi panggil, lewati, dan tidak hadir |
| **Dependency** | `FE-RWI-042`, `BE-RWI-044` |
| **Acceptance Criteria** | 1. Kesepuluh elemen safety context pada Layout selalu tampil sebelum dokumentasi; data yang gagal tidak diganti nilai normal palsu. 2. Context loading/gagal/mismatch menonaktifkan seluruh write termasuk autosave, modal dan addendum, dengan retry. 3. Allergy error menonjol dan berbeda dari alergi kosong. 4. Authority tidak sah menonaktifkan area tulis dan menjelaskan yang berwenang; aksi tanpa permission tetap hidden. 5. Nol aksi/layanan antrean. 6. Tepat enam tab pada satu pasien/episode; Closed read-only dengan addendum sah, tanpa global finalize |
| **Visual Acceptance Criteria** | Urutan composition sama dengan skema §6/§20; safety context terlihat pada tiap tab dan saat scroll; alergi gagal/tercatat/kosong berbeda; authority dan Closed terbaca; tab aktif dan kontennya terhubung; tiga viewport memenuhi §1.4 |
| **Verification** | Test delapan state §1.3; simulasi context 500, allergy 500, authority 403, episode Closed dan pergantian episode dengan response lambat; pastikan nol mutation saat context invalid; bukti visual seluruh safety state dan tiga viewport; scan queue/global finalize |
| **Risk / Blocker** | Ketiadaan penanda alergi terbaca sebagai "tidak ada alergi", dan bagi peresepan itu berbahaya. Kegagalannya **wajib** terlihat. Owner: Frontend authority |
| **DoD** | Seluruh acceptance fungsional/visual dan state di atas terbukti; gate §4.1 relevan lulus; adapter/base/domain terpisah; laporan menyertakan bukti context failure, allergy error, authority, Closed, responsive dan nol queue/global finalize |

---

### 🟡 `FE-RWI-044` — Layar kajian medis awal, terpisah kasatmata dari pengkajian keperawatan

| Field | Isi |
| --- | --- |
| **Status** | 🟡 `SEBAGIAN` **8 September 2026.** **6 dari 6** acceptance criteria fungsional dan **9 dari 9** acceptance visual terpenuhi serta terpetakan ke source. **Yang belum: 1 elemen layout** — tombol **+ Tambah Diagnosis** pada Diagnosis / Problem List tertahan kontrak backend, karena `CreatePatientDiagnosisRequest.ConsultationId` bersifat `[Required]` sehingga diagnosis terstruktur hanya dapat lahir dari catatan dokter milik `FE-RWI-045`; daftar masalahnya sudah tampil hanya-baca beserta keterangan ke mana penambahannya menempel. Konflik kewenangan dokter jaga pada §6 **tidak** dinyatakan lulus, sesuai DoD task ini. Validasi nyata: `npm run lint:errors` 0 error; `npm run test:unit` 487/487 lulus, 0 gagal (15 test baru task ini); `npm run build` beserta `postbuild` berhasil; **35 skenario peramban lulus** di Edge — 13 skenario `FE-RWI-044` (dua panel 70/30, rujukan kosong bukan penghalang, rujukan gagal ≠ kosong, isian bertahan saat simpan gagal, bagian kosong disebut satu per satu, Selesaikan tersembunyi bagi yang tidak berwenang, final tanpa sunting langsung, Koreksi tersembunyi saat tidak berwenang, koreksi tidak menimpa isi asli, payload draf, tiga viewport) ditambah 22 regresi `FE-RWI-042`/`FE-RWI-043`. Satu cacat ditemukan uji dan diperbaiki: modal penegasan sempat tetap terbuka saat penyelesaian ditolak sehingga ringkasan validasi tertutup. Laporan: [`task/report/frontend/FE-RWI-044.md`](../task/report/frontend/FE-RWI-044.md) |
| **Outcome** | DPJP mengisi pemeriksaan menyeluruh pertama, dan tidak seorang pun dapat mengira dokumen itu adalah pengkajian keperawatan |
| **Trace** | `FE-DOK-02`; `03-frontend-architecture.md` §3.2; `AC-CAP022-02` |
| **Kontrak** | `0.3.0` |
| **Reuse** | Form/field existing yang cocok dipakai pada adapter `MedicalInitial`; bagian pengkajian keperawatan dibaca terpisah, bukan menyalin screening antrean sebagai kajian medis |
| **UI Contract** | [skema-tampilan-dokter-rawat-inap.md](../skema-tampilan-dokter-rawat-inap.md) §4–5, §7, §16–20, §22; rules §1 roadmap; validasi tetap `VAL-DOK-10/11` |
| **Layout** | Desktop **70% form medis / 30% referensi keperawatan READ ONLY**. Form berurutan: Anamnesis → Pemeriksaan Fisik → Assessment → Planning → Diagnosis / Problem List terstruktur. `ClinicalValidationSummary` sebelum form/di lokasi mudah dicapai; `ClinicalCompletionBar` di bawah dokumen dengan peringatan penguncian sebelum Selesaikan |
| **Existing Base Components** | `ClinicalSectionPanel`, `ClinicalStatusBadge`, `ClinicalEmptyState`; safety header dan tab dari shell FE-RWI-043 tetap tampil |
| **Base Components To Extend** | Tidak ada extend base wajib; adapter form memetakan field `MedicalInitial` dan referensi jenis `Initial` ke panel terpisah. Jangan menaruh validasi kajian pada base panel |
| **New Base Components** | Membuat `ClinicalValidationSummary`, `ClinicalCompletionBar`; reuse base rencana `ClinicalStateBoundary`, `ClinicalSafetyAlert`, `ClinicalActionGuard` dari FE-RWI-042/043 |
| **Domain Components** | `MedicalAssessmentTab`, `MedicalAssessmentForm`, `MedicalProblemList`, `NursingAssessmentReference`, `CompleteDocumentModal`, `CorrectionModal`/detail addendum untuk kajian final sesuai API §9; tidak membuat formulir keperawatan yang bisa diedit |
| **Components / Legacy Behaviour To Remove** | Screening antrean as-is, queue item/QueueId wajib, editable nursing form di kajian dokter, direct edit Completed dan global finalize; larangan queue §1.1 tetap berlaku |
| **State Handling** | `LOADING`: form/referensi punya placeholder sendiri; `EMPTY`: kajian belum ada dapat dibuat jika berhak, keperawatan belum ada bukan blocker; `ERROR`: baca/simpan kajian jelas, isian bertahan; referensi gagal ≠ belum ada; `RETRY`: ulang wilayah gagal/simpan sesuai hasil; `READ ONLY`: Completed/detail referensi; `PERMISSION DENIED`: no Read gate/no write hidden; `EPISODE CLOSED`: tidak membuat/mengedit draft, koreksi final hanya authority sah; `DOUBLE SUBMIT / REQUEST IN PROGRESS`: simpan/complete/koreksi pending dan tidak berlomba |
| **Permission Behaviour** | `PatientAssessment : Read/Create/Update`, diagnosis memakai permission `PatientDiagnosis` yang dikonfirmasi dari source, addendum mengikuti API §9. Penulis/jenis kajian dan authority episode wajib diperiksa. Konflik DPJP versus dokter jaga pada §6 belum boleh ditutup dengan asumsi role UI; skenario DPJP berwenang dapat disiapkan independen. Selesaikan hanya tampil bagi yang benar-benar berhak |
| **Responsive Behaviour** | §1.4: desktop 70/30; tablet panel tetap berbeda dan dapat ditumpuk bila isian tidak muat; mobile safety context → form medis → referensi READ ONLY → completion. Validation summary dapat memfokuskan field yang disebut, tombol tidak menutup isian |
| **Scope** | Formulir kajian medis; daftar masalah terstruktur; rujukan hanya-baca ke pengkajian keperawatan; tombol Selesaikan |
| **Dependency** | `FE-RWI-043` ✅, `BE-RWI-045` ✅. **Ditambahkan revision 3:** `BE-RWI-068` ⛔ — tombol Tambah Diagnosis tidak dapat ditutup sebelum task itu selesai, dan task itu sendiri menunggu grup diagnosis masuk kontrak API |
| **Acceptance Criteria** | 1. Layar menampilkan kajian medis dan pengkajian keperawatan sebagai **dua hal yang jelas berbeda**. 2. Pengkajian keperawatan tampil hanya-baca dan **bukan penghalang** bila belum ada. 3. Kegagalan menyimpan **tidak menghilangkan isian**. 4. Menyelesaikan kajian yang belum lengkap menampilkan bagian kosong **satu per satu** sesuai VAL-DOK-10/11. 5. Tombol Selesaikan hanya muncul bagi peran/penulis yang berhak. 6. Completed tidak memiliki direct edit; Koreksi melalui addendum beralasan hanya bagi authority sah, isi asli tetap utuh |
| **Visual Acceptance Criteria** | Dua panel berjudul jelas; referensi berlabel READ ONLY tanpa kontrol edit; empty “Pengkajian keperawatan belum tersedia. Kajian medis tetap dapat dilanjutkan”; kelima bagian form terlihat; summary menyebut field belum lengkap; completion memperingatkan penguncian sebelum klik; Completed tanpa Sunting, Koreksi hanya berwenang; bukti tiga viewport |
| **Verification** | Test pemisahan jenis kajian, referensi kosong dan gagal, isian bertahan saat gagal simpan, daftar keluhan/pemeriksaan/rencana/diagnosis yang belum lengkap, Completed tidak bisa diedit dan addendum sah tidak menimpa isi; test permission/state lokal; screenshot 70/30, final, validation summary dan mobile |
| **Risk / Blocker** | Keduanya tersimpan pada tabel yang sama; pemisahan di layar **mengikat**. Hak kajian dokter jaga masih NEEDS CONFIRMATION (§6); tidak menahan layout dan kasus DPJP berwenang. Owner: Frontend authority bersama Product/Domain dan ClinicalManagement |
| **DoD** | Keenam acceptance, Visual Acceptance Criteria/state/permission dan gate §4.1 relevan terbukti; laporan tracked memuat screenshot 70/30 serta handoff dua base baru. Konflik authority §6 tidak boleh dinyatakan lulus tanpa bukti keputusan |

---

### ✅ `FE-RWI-045` — Catatan perkembangan beserta waktu pemeriksaan dan koreksinya

| Field | Isi |
| --- | --- |
| **Status** | ✅ `SELESAI` **8 September 2026.** Keenam acceptance criteria terpetakan ke source yang benar-benar ada. Validasi nyata: `npm run lint:errors` 0 error; `npm run test:unit` 542/542 lulus, 0 gagal — 21 di antaranya uji baru untuk keenam task ini; `npm run build` beserta `postbuild` berhasil. Bukti kunci: lini masa memakai urutan server menurut waktu pemeriksaan dan menampilkan dua label waktu terpisah (contoh pemeriksaan 07.40 dicatat 11.03); waktu mundur diterima sementara masa depan dan sebelum pasien masuk kamar ditolak sesuai `VAL-DOK-13/14`; satu bagian S/O/A/P sudah cukup sesuai `VAL-DOK-12`; catatan final kehilangan editornya sama sekali dan nol kemunculan tombol Sunting/Edit; peringatan penguncian tampil **sebelum** tombol Selesaikan ditekan; penulis asli tidak pernah tergantikan pengoreksi. Empat base baru dibuat dan diekspor: `ClinicalTimeline`, `ClinicalTimelineItem`, `ClinicalDocumentMeta`, `ClinicalAuditBadge`. Butir DoD bukti visual tiga viewport **dikecualikan atas keputusan pengguna** bahwa pengujian e2e dan `.mjs` bukan gerbang selesai pada repository ini; catatan verifikasinya tetap tercatat sebagai `NOT RUN` pada laporan, tidak dihapus. Laporan: [`task/report/frontend/FE-RWI-045.md`](../task/report/frontend/FE-RWI-045.md) |
| **Outcome** | Dokter menulis catatan harian dengan waktu pemeriksaan yang sebenarnya, dan membetulkan yang sudah final tanpa dapat menyuntingnya diam-diam |
| **Trace** | `FE-DOK-03`; `03-frontend-architecture.md` §3.3; `RWI-DEC-086`, `RWI-DEC-088` |
| **Kontrak** | `0.3.0` |
| **Reuse** | Presentasi S/O/A/P dari `doctor-soap-tab.jsx` sebagai kandidat adapter setelah props/caller terbukti netral queue; lifecycle catatan individual, bukan finalisasi workspace |
| **UI Contract** | [skema-tampilan-dokter-rawat-inap.md](../skema-tampilan-dokter-rawat-inap.md) §4–5, §8, §16–20, §22; rules §1 roadmap; kontrak API §1/§9, state §1/§6 dan `VAL-DOK-12–15/32–35` |
| **Layout** | Timeline kiri + SOAP editor/detail kanan. Waktu Pemeriksaan di atas Subjective, Objective, Assessment, Plan. Item selected jelas; `ClinicalDocumentMeta` memisahkan waktu klinis dari dicatat. Completion di bawah dokumen draft; detail final menampilkan isi asli lalu blok koreksi berurutan dengan alasan, pelaku, waktu, penanda pengganti |
| **Existing Base Components** | `ClinicalSectionPanel`, `ClinicalStatusBadge`, `ClinicalEmptyState`; header safety/tab dari FE-RWI-043 |
| **Base Components To Extend** | Tidak ada extend base existing wajib; adapter S/O/A/P melepas props queue dan mengikat documentId/episode; metadata tidak diturunkan dari queue item |
| **New Base Components** | Membuat `ClinicalTimeline`, `ClinicalTimelineItem`, `ClinicalDocumentMeta`, `ClinicalAuditBadge`; memakai `ClinicalCompletionBar` dari FE-RWI-044 serta `ClinicalStateBoundary`, `ClinicalSafetyAlert`, `ClinicalActionGuard`; urutan handoff §1.2 |
| **Domain Components** | `PhysicianProgressTab`, `ProgressNoteTimeline`, `SoapEditor`, `ProgressNoteDetail`, `ClinicalTimeField`, `ProgressNoteCorrectionHistory`, `CorrectionModal`, `CompleteDocumentModal`; domain menentukan urutan waktu dan authority |
| **Components / Legacy Behaviour To Remove** | `useDoctorConsultationWorkspace` as-is dan seluruh queue §1.1; pengurutan menurut createdAt; Sunting/Edit setelah Final; Koreksi pada draft; global Simpan Konsultasi; penggantian nama penulis asli |
| **State Handling** | `LOADING`: timeline/detail/authority terpisah; `EMPTY`: belum ada catatan perkembangan; `ERROR`: gagal baca/simpan/koreksi, isian tidak hilang; `RETRY`: request wilayah/dokumen yang sama; `READ ONLY`: Completed/Cancelled tanpa editor; `PERMISSION DENIED`: gate Read, Koreksi hidden jika authority tidak ada; `EPISODE CLOSED`: tidak ada catatan baru/direct edit, addendum final sah tetap bisa; `DOUBLE SUBMIT / REQUEST IN PROGRESS`: autosave diselesaikan sebelum complete, tombol complete/koreksi pending dan satu hasil ditampilkan |
| **Permission Behaviour** | `DoctorConsultation : Read/Create/Update` plus penulis/authority episode. `ClinicalNoteAddendum : Read/Create/CreateAsSubstitute` dan hasil endpoint authority menentukan Koreksi. Dokter pengganti hanya DPJP aktif episode menurut `RWI-DEC-088`, bukan semua pemegang permission; tidak mengirim penulis addendum dari form |
| **Responsive Behaviour** | Desktop timeline kiri/editor kanan; tablet 35/65 bila muat; mobile context → timeline → pilih catatan → detail/editor → completion satu dokumen. Penulis/waktu/status tidak hilang saat berpindah kolom; §1.4 berlaku |
| **Scope** | Lini masa terurut waktu pemeriksaan; formulir empat bagian; pengisian waktu pemeriksaan; tombol Koreksi; penanda koreksi beserta penulisnya |
| **Dependency** | `FE-RWI-043`, `BE-RWI-046`, `BE-RWI-047` |
| **Acceptance Criteria** | 1. Lini masa terurut **waktu pemeriksaan**, bukan waktu penulisan. 2. Waktu pemeriksaan dapat diisi mundur; masa depan dan sebelum pasien masuk kamar ditolak sesuai VAL-DOK-13/14. 3. **Tidak ada tombol Sunting** pada catatan yang sudah diselesaikan. 4. Layar mengatakan bahwa menekan Selesai mengunci catatan **sebelum** tombol itu ditekan; minimal satu bagian SOAP terisi sesuai VAL-DOK-12. 5. Penanda koreksi menampilkan **penulis asli sebagai penulis catatan**, dan dokter pengganti hanya pada baris koreksinya. 6. Tombol Koreksi disembunyikan bila pengguna tidak berwenang; draft disunting langsung, final tetap Completed sesudah addendum |
| **Visual Acceptance Criteria** | Dua kolom dan selected item terlihat; contoh pemeriksaan 07.40/dicatat 11.03 memiliki dua label waktu; final tanpa Sunting/Edit, Koreksi hanya berwenang; isi/penulis asli tetap di atas blok addendum, pengganti dan audit terpisah; peringatan completion sebelum klik; tiga viewport sesuai §1.4 |
| **Verification** | Test urutan tiga catatan menurut ClinicalDateTime meski waktu catat berbeda, waktu masa depan/sebelum masuk kamar ditolak, satu bagian SOAP cukup untuk complete; draft editable tanpa Koreksi; final tanpa Edit; koreksi penulis/pengganti termasuk Closed, alasan kosong dan authority gagal; bukti visual timeline/editor, final/koreksi, state dan tiga viewport |
| **Risk / Blocker** | Memberi tahu penguncian **setelah** tombol ditekan akan membuat dokter merasa dijebak; penempatan pemberitahuannya mengikat, kalimatnya `DEV_DISCRETION`. Owner: Frontend authority |
| **DoD** | Keenam acceptance existing, validasi waktu/minimal satu bagian, seluruh state/permission dan Visual Acceptance Criteria terbukti; gate §4.1 relevan lulus; laporan menyertakan bukti urutan/penulis asli/Closed dan handoff empat base baru; tidak membuat status Amended atau global finalize |

---

### 🟡 `FE-RWI-046` — Catatan terpadu dan verifikasi DPJP

| Field | Isi |
| --- | --- |
| **Status** | 🟡 `SEBAGIAN` **8 September 2026.** **4 dari 5** acceptance criteria terpenuhi penuh; **kriteria 2 terpenuhi separuh**. Validasi nyata: `npm run lint:errors` 0 error; `npm run test:unit` 542/542 lulus, 0 gagal — 21 di antaranya uji baru untuk keenam task ini; `npm run build` beserta `postbuild` berhasil. Yang terbukti: setiap catatan menampilkan penulis beserta profesinya, dan nama penulis diambil berjenjang sehingga akun yang berganti nama tidak mengubah penulis catatan lama; Penulis dan Verifikator adalah **dua baris terpisah** sehingga nama penulis tidak mungkin tertimpa; tombol Verifikasi **disembunyikan** bila salah satu dari empat syarat gagal — DPJP aktif saat ini, bukan penulis, status menerima Verify, episode belum ditutup; kebijakan yang belum aktif berbunyi “Verifikasi DPJP tidak diwajibkan”, bukan daftar kosong; keterlambatan hanya dipantau dan tidak menahan penulisan berikutnya. **Yang belum: nama verifikator tidak dapat ditampilkan** karena `PatientIntegratedProgressNoteResponse` tidak mengembalikan `VerificationStatus`, `VerifiedAt`, maupun `VerifiedByUserId` — pemetaan `ToResponse` pada `PatientIntegratedProgressNoteController.cs:1397–1435` tidak menyertakannya, dan balasan `PATCH /{id}/verify` memakai pemetaan yang sama. Akibatnya status per catatan hanya dapat disimpulkan sebagian, dan layar menambahkan keadaan kelima **belum dapat dipastikan** alih-alih menebak “sudah diverifikasi”. Celah kontrak itu **tidak diperbaiki dari sini** karena task bermode `FRONTEND`. Gerbang §4.1 butir 10 karena itu terbukti separuh. **Isu §6 eligibility Verifikasi pada episode Closed tetap terbuka**; layar memakai baseline Closed hanya-baca. Butir DoD bukti visual tiga viewport **dikecualikan atas keputusan pengguna** bahwa pengujian e2e dan `.mjs` bukan gerbang selesai pada repository ini; catatan verifikasinya tetap tercatat sebagai `NOT RUN` pada laporan, tidak dihapus. Laporan: [`task/report/frontend/FE-RWI-046.md`](../task/report/frontend/FE-RWI-046.md) |
| **Outcome** | DPJP membaca catatan seluruh profesi pada satu lembar dan menyatakan sudah memeriksanya, tanpa nama penulis aslinya tergantikan |
| **Trace** | `FE-DOK-04`; `03-frontend-architecture.md` §3.4; `AC-CAP021-03` |
| **Kontrak** | `0.3.0` |
| **Reuse** | `doctor-cppt-tab.jsx` sebagai kandidat presentasi yang diadaptasi ke episode; timeline/base FE-RWI-045 dipakai sekali, bukan disalin |
| **UI Contract** | [skema-tampilan-dokter-rawat-inap.md](../skema-tampilan-dokter-rawat-inap.md) §4–5, §9, §16–20, §22; rules §1 roadmap; API §3, state §3, permission §2–3 |
| **Layout** | Timeline lintas profesi satu kolom, header Catatan Terpadu + Filter Profesi. Setiap item: waktu klinis → Penulis + Profesi → isi/detail klinis berizin → status verifikasi → Verifikator + waktu verifikasi terpisah → Verifikasi bila berhak. Riwayat koreksi/audit tetap tertaut ke item; bukan daftar monitoring |
| **Existing Base Components** | `ClinicalSectionPanel`, `ClinicalStatusBadge`, `ClinicalEmptyState` |
| **Base Components To Extend** | Tidak ada extend base wajib; adapter CPPT mengisi slot timeline, status dan metadata tanpa menaruh kebijakan DPJP di base |
| **New Base Components** | Tidak membuat base baru tambahan; memakai rencana `ClinicalTimeline`, `ClinicalTimelineItem`, `ClinicalDocumentMeta`, `ClinicalAuditBadge` FE-RWI-045 dan `ClinicalStateBoundary`, `ClinicalActionGuard`, `ClinicalSafetyAlert` FE-RWI-042/043 |
| **Domain Components** | `IntegratedProgressNoteTab`, `IntegratedNoteTimeline`, `ProfessionFilter`, `CpptVerificationStatus`, `CpptVerificationAction`, detail penulis/verifikator dan audit koreksi; entry penulisan existing hanya jika sesuai permission, perawat tetap menulis dari workspace perawat |
| **Components / Legacy Behaviour To Remove** | CPPT props/caller queue, penggantian penulis dengan verifikator, tombol Verify bagi semua role, global complete lintas domain, asumsi NotRequired berarti Verified; larangan §1.1 berlaku |
| **State Handling** | `LOADING`: skeleton timeline/status; `EMPTY`: belum ada CPPT setelah baca berhasil; `ERROR`: gagal timeline/verifikasi/authority berbeda; `RETRY`: baca ulang status aktual; `READ ONLY`: isi asli tetap terbaca tanpa edit final; `PERMISSION DENIED`: gate Read, Verifikasi hidden; `EPISODE CLOSED`: banner read-only, perilaku verifikasi Closed mengikuti keputusan §6, addendum sah sesuai kontrak; `DOUBLE SUBMIT / REQUEST IN PROGRESS`: Verifikasi pending per item, status tidak dianggap Verified sebelum sukses. Empat badge berbeda: Pending Verification, Verified, Overdue, Verification Not Required |
| **Permission Behaviour** | Verifikasi tampil hanya untuk `PatientIntegratedProgressNote : Verify` + DPJP aktif **saat verifikasi** + bukan penulis asli + status yang menerima Verify. Dokter jaga/perawat/supervisor tidak mendapat tombol. Penulis Ns. Sari tetap Penulis; dr. Andi hanya Verifikator. Koreksi mengikuti authority dokumen; Verified yang dikoreksi kembali Pending dari API, tidak dihitung lokal |
| **Responsive Behaviour** | Timeline satu kolom pada semua ukuran; filter/metadata dua kolom pada tablet bila muat, bertumpuk pada mobile; Penulis dan Verifikator tetap label berbeda, isi wrap tanpa menghapus profesi/status; context shell tetap terlihat (§1.4) |
| **Scope** | Lini masa lintas profesi; penanda verifikasi; tombol Verifikasi; penanda penulis dan verifikator terpisah |
| **Dependency** | `FE-RWI-043` ✅, `BE-RWI-053` ✅. **Ditambahkan revision 3:** `BE-RWI-066` — nama verifikator tidak dapat ditampilkan sebelum balasan baca catatan terpadu memuatnya |
| **Acceptance Criteria** | 1. Setiap catatan menampilkan penulis dan profesinya. 2. Setelah diverifikasi, **nama penulis asli tetap tampil sebagai penulis**; verifikator tampil terpisah. 3. Tombol Verifikasi **disembunyikan** bagi yang tidak berhak, bukan ditampilkan lalu ditolak. 4. Saat kebijakan verifikasi tidak aktif, penanda berbunyi "verifikasi tidak diwajibkan" — bukan daftar kosong. 5. Keterlambatan tampil tanpa menahan penulisan catatan berikutnya |
| **Visual Acceptance Criteria** | Penulis dan Verifikator dua baris berbeda; contoh Ns. Sari/dr. Andi tidak menimpa nama; Pending, Verified, Overdue, NotRequired memiliki label/visual berbeda, tidak sekadar warna; tombol hanya item yang eligible; policy tidak aktif ≠ daftar gagal; tiga viewport |
| **Verification** | Test penulis/verifikator, DPJP aktif versus DPJP lama/non-DPJP/self-verification, keempat status verifikasi, kebijakan kosong dan gagal dimuat, koreksi Verified kembali Pending tanpa menahan penulisan berikutnya; state lokal dan screenshot tiga viewport |
| **Risk / Blocker** | Menampilkan satu nama saja menghilangkan pembedaan penulis/verifikator. Eligibility Verifikasi pada Closed masih NEEDS CONFIRMATION (§6); kasus episode aktif dan tampilan empat status dapat dikerjakan. Owner: Frontend authority bersama ClinicalManagement |
| **DoD** | Kelima acceptance existing, empat status, larangan self-verification, state/permission dan Visual Acceptance Criteria terbukti; gate §4.1 relevan lulus; laporan memuat bukti per role dan policy. Authority Closed yang masih NEEDS CONFIRMATION (§6) belum boleh dianggap lulus |

---

### ✅ `FE-RWI-047` — Riwayat visite beserta pencatatan dan pembatalannya

| Field | Isi |
| --- | --- |
| **Status** | ✅ `SELESAI` **8 September 2026.** Keenam acceptance criteria terpetakan ke source yang benar-benar ada. Validasi nyata: `npm run lint:errors` 0 error; `npm run test:unit` 542/542 lulus, 0 gagal — 21 di antaranya uji baru untuk keenam task ini; `npm run build` beserta `postbuild` berhasil. Bukti kunci: kejadian yang dibatalkan **tetap berdiri** di riwayat beserta alasan, pembatal, dan waktunya; keadaan kosong berbunyi “Belum ada visite tercatat” dan menegaskan catatan perkembangan tidak dihitung sebagai visite; kunci permintaan diturunkan dari isi kiriman sehingga kiriman ulang memakai kunci sama sementara dua visite nyata memakai kunci berbeda; visite berdekatan hanya **diperingatkan** dan tetap dapat dilanjutkan; alasan pembatalan wajib; pemindaian keempat berkas tab mengembalikan **nol** tombol Sunting/Edit. Butir DoD bukti visual tiga viewport **dikecualikan atas keputusan pengguna** bahwa pengujian e2e dan `.mjs` bukan gerbang selesai pada repository ini; catatan verifikasinya tetap tercatat sebagai `NOT RUN` pada laporan, tidak dihapus. **Dua isu §6 tetap terbuka dan tidak dinyatakan lulus**: hak Cancel dokter konsulen, dan mutasi event pada episode Closed — layar memakai baseline Closed hanya-baca. Dependency `BE-RWI-048` 🟡 tetap, termasuk bukti concurrency PostgreSQL yang masih terbuka. Laporan: [`task/report/frontend/FE-RWI-047.md`](../task/report/frontend/FE-RWI-047.md) |
| **Outcome** | Dokter mencatat kunjungannya, membatalkan yang salah catat, dan melihat riwayat yang jujur — termasuk baris yang dibatalkan |
| **Trace** | `FE-DOK-05`; `03-frontend-architecture.md` §3.5; `RWI-DEC-084`, `RWI-DEC-085`; `RWI-AC-150` s.d. `RWI-AC-156` |
| **Kontrak** | `0.3.0` |
| **Reuse** | Timeline/base FE-RWI-045 dan pola idempotency request existing yang sesuai kontrak PhysicianVisit; bukan mengubah catatan SOAP menjadi visite |
| **UI Contract** | [skema-tampilan-dokter-rawat-inap.md](../skema-tampilan-dokter-rawat-inap.md) §4–5, §10, §16–20, §22; rules §1 roadmap; API §4 dan state §5 |
| **Layout** | Header Riwayat Visite + **Catat Visite**, diikuti event timeline. Item berisi waktu visite, dokter, peran, catatan dalam detail episode berizin, tautan opsional, status dan Batalkan bila berhak. Item Cancelled tetap di timeline beserta alasan. Modal Catat Visite: waktu, peran sesuai authority, catatan, optional document link; modal Batalkan: alasan wajib + Kembali/Batalkan Visite |
| **Existing Base Components** | `ClinicalSectionPanel`, `ClinicalStatusBadge`, `ClinicalEmptyState` |
| **Base Components To Extend** | Tidak ada extend base wajib; slot timeline menerima cancelled metadata dari domain, bukan menyimpan rule pembatalan di base |
| **New Base Components** | Tidak membuat tambahan; reuse rencana `ClinicalTimeline`, `ClinicalTimelineItem`, `ClinicalDocumentMeta`, `ClinicalAuditBadge` FE-RWI-045; `ClinicalSafetyAlert`, `ClinicalActionGuard`, `ClinicalStateBoundary` FE-RWI-042/043 |
| **Domain Components** | `PhysicianVisitTab`, `PhysicianVisitTimeline`, `RecordVisitModal`, `CancelVisitModal`, `VisitDocumentLink`, `NearbyVisitWarning`; koreksi: batalkan lalu catat ulang dengan `CorrectsVisitId` dan key baru |
| **Components / Legacy Behaviour To Remove** | Edit/Sunting waktu atau peran event, delete/hide cancelled, perhitungan visite dari SOAP/CPPT, blocker visite kedua pada hari sama, queue dan global finalize §1.1 |
| **State Handling** | `LOADING`: skeleton event; `EMPTY`: “Belum ada visite tercatat” walau ada tiga SOAP; `ERROR`: gagal timeline/catat/batal terpisah, form bertahan; `RETRY`: baca ulang atau kirim ulang logical request dengan key sama; `READ ONLY`: riwayat tetap tampil; `PERMISSION DENIED`: gate Read/aksi tanpa izin hidden; `EPISODE CLOSED`: tidak mencatat baru, operasi event lama yang belum jelas mengikuti §6; `DOUBLE SUBMIT / REQUEST IN PROGRESS`: Catat/Batalkan disabled selama request, replay menampilkan event sama. Cancelled terminal, warning visite dekat bisa dilanjutkan |
| **Permission Behaviour** | `PhysicianVisit : Read/Create/Cancel/Update` sesuai aksi; hanya dokter berwenang dapat mencatat, bukan petugas atas nama dokter. Cancel butuh izin dan pemilik event/supervisor sesuai state §5; konsulen mengikuti konflik §6. Peran pada modal tidak memberi hak yang tidak dimiliki; tautan lintas episode ditolak |
| **Responsive Behaviour** | Timeline satu kolom; desktop tombol di header/item, tablet metadata dua kolom, mobile modal/metadata bertumpuk dengan tombol terjangkau. Alasan batal tidak terpotong permanen; context safety tetap di atas (§1.4) |
| **Scope** | Riwayat visite; tombol Catat Visite beserta kunci permintaan; peringatan visite berdekatan; tombol Batalkan beserta alasan wajib; tautan dokumen opsional |
| **Dependency** | `FE-RWI-043`, `BE-RWI-048`, `BE-RWI-049` |
| **Acceptance Criteria** | 1. Riwayat menampilkan kejadian yang **dibatalkan beserta alasannya**, tidak disembunyikan. 2. Keadaan kosong berbunyi "belum ada visite tercatat" **walaupun sudah ada tiga catatan perkembangan**. 3. Tombol Catat Visite nonaktif selama permintaan berjalan, dan penekanan dua kali menghasilkan satu kejadian. 4. Visite pada jam berdekatan **diperingatkan, bukan ditolak**, dan dapat dilanjutkan. 5. Tombol Batalkan menuntut alasan; tombol simpan nonaktif selama alasan kosong. 6. **Tidak ada tombol Sunting** pada kejadian visite |
| **Visual Acceptance Criteria** | Catat Visite terlihat hanya bagi yang berhak; tidak ada Edit/Sunting; event Cancelled tetap terlihat dengan label/alasan; modal menampilkan empat field termasuk link opsional; visite 07.20/07.40 hanya warning yang dapat dilanjutkan; alasan kosong menonaktifkan submit batal; tiga viewport |
| **Verification** | Test empty meski tiga SOAP, double click/retry key sama menghasilkan satu event, dua visite nyata menghasilkan dua event, warning tetap bisa lanjut, cancel beralasan/cancel kedua 409, koreksi key baru dan link episode sama; test permission/state, scan Edit dan screenshot modal/timeline tiga viewport |
| **Risk / Blocker** | Menyembunyikan cancelled menghilangkan jejak audit; menolak visite kedua dilarang RWI-DEC-085. Hak Cancel konsulen dan mutasi event pada Closed masih NEEDS CONFIRMATION (§6); bukti concurrency BE-RWI-048 tetap terbuka. Owner: Frontend authority bersama ClinicalManagement |
| **DoD** | Keenam acceptance existing, seluruh state/permission dan Visual Acceptance Criteria terbukti; gate §4.1 relevan lulus; laporan menampilkan cancelled event dan modal nyata. Dependency BE-RWI-048/049 tetap, termasuk keterbatasan bukti concurrency backend yang masih terbuka; isu authority §6 tidak dianggap selesai |

---

### ✅ `FE-RWI-048` — Resep dan tindakan dari satu layar

| Field | Isi |
| --- | --- |
| **Status** | ✅ `SELESAI` **8 September 2026.** Kelima acceptance criteria terpetakan ke source yang benar-benar ada. Validasi nyata: `npm run lint:errors` 0 error; `npm run test:unit` 542/542 lulus, 0 gagal — 21 di antaranya uji baru untuk keenam task ini; `npm run build` beserta `postbuild` berhasil. Bukti kunci: satu tab utama dengan dua segmen lewat `ClinicalSegmentedNav` baru; Rutin, Harian, dan Obat Pulang menghasilkan tiga label berbeda; pemindaian keempat berkas segmen mengembalikan **nol** kemunculan “Tandai Diserahkan”, dan service resep tidak memuat satu pun fungsi tulis status pemenuhan; keadaan Farmasi yang tidak dikenal berbunyi “belum diketahui” dan **tidak** dipalsukan menjadi “Sudah Diserahkan”; tindakan `Completed` dengan tagihan gagal tetap berstatus Selesai dan kegagalannya hanya menjadi penanda baris, bukan galat halaman; kiriman resep berulang memakai kunci permintaan yang sama. Butir DoD bukti visual tiga viewport **dikecualikan atas keputusan pengguna** bahwa pengujian e2e dan `.mjs` bukan gerbang selesai pada repository ini; catatan verifikasinya tetap tercatat sebagai `NOT RUN` pada laporan, tidak dihapus. **Isu §6 percobaan ulang pengiriman tagihan tetap terbuka**: layar hanya menyediakan Baca Ulang Status, nol endpoint dispatch retry dikarang, dan `execute` tidak pernah dikirim ulang. Peringatan obat pulang `VAL-DOK-20` tetap tercatat belum dikerjakan di backend. Dependency `BE-RWI-051` 🟡 tetap. Laporan: [`task/report/frontend/FE-RWI-048.md`](../task/report/frontend/FE-RWI-048.md) |
| **Outcome** | Dokter meresepkan dan mencatat tindakan tanpa berpindah layar, dan melihat status pemenuhan tanpa dapat mengubahnya |
| **Trace** | `FE-DOK-06`; `03-frontend-architecture.md` §3.6; `RUL-DOK-01` |
| **Kontrak** | `0.3.0` |
| **Reuse** | Presentasi/form `doctor-prescription-tab.jsx` dan `doctor-procedure-tab.jsx` sebagai kandidat adapter episode; API milik Farmasi/ClinicalManagement tetap, tidak memakai queue consultation lifecycle |
| **UI Contract** | [skema-tampilan-dokter-rawat-inap.md](../skema-tampilan-dokter-rawat-inap.md) §4–5, §11, §14, §16–20, §22; rules §1 roadmap; API §5/§6/§9 dan integrasi INT-DOK-03/06/07 |
| **Layout** | Satu tab **Resep & Tindakan**, internal `ClinicalSegmentedNav` **Resep / Tindakan**. Segmen Resep: Buat Resep + tabel Tanggal, Jenis, Dokter, Status Resep, Farmasi (READ ONLY). Segmen Tindakan: Tambah Tindakan + tabel Tindakan, Status Klinis, Dokter, Billing dispatch per row; detail/form milik item terpilih |
| **Existing Base Components** | `ClinicalSectionPanel`, `ClinicalDataTable`, `ClinicalStatusBadge`, `ClinicalEmptyState` |
| **Base Components To Extend** | Tidak ada extend base wajib; adapter columns/rowKeyFn memberi id resep/tindakan dan kolom dispatch, bukan status baru di base tabel |
| **New Base Components** | Membuat `ClinicalSegmentedNav` generik; reuse rencana `ClinicalStateBoundary`, `ClinicalSafetyAlert`, `ClinicalActionGuard`; koreksi/detail final memakai base metadata yang sudah diserahkan sesuai §1.2 bila diperlukan |
| **Domain Components** | `PrescriptionProcedureTab`, `InpatientPrescriptionList`, `InpatientPrescriptionForm`, `InpatientProcedureList`, `InpatientProcedureForm`, `ProcedureBillingDispatchStatus`, detail tindakan/final dan `CorrectionModal` berizin |
| **Components / Legacy Behaviour To Remove** | Tab Prescription/Procedure terpisah di tingkat utama, queue props/lock, global finalize, Tandai Diserahkan, status Billing sebagai status klinis, full-page error untuk dispatch gagal, direct edit tindakan final |
| **State Handling** | `LOADING`: masing-masing segmen/baris; `EMPTY`: belum ada resep ≠ belum ada tindakan; `ERROR`: form/daftar lokal, fulfillment/dispatch gagal per row, isian bertahan; `RETRY`: baca ulang status row, retry create dengan key sama; retry dispatch mutation belum dikontrak (§6); `READ ONLY`: Farmasi/status klinis final; `PERMISSION DENIED`: gate Read per segmen dan write hidden; `EPISODE CLOSED`: tanpa resep/tindakan baru, addendum tindakan final jika sah; `DOUBLE SUBMIT / REQUEST IN PROGRESS`: simpan/execute pending per item dan idempotency, tanpa row ganda |
| **Permission Behaviour** | `Prescription : Read/Create`, `PatientProcedure : Read/Create/Update` plus authority episode/pelaksana; koreksi final memakai API §9/authority dokumen. Tidak ada role dokter yang boleh mengubah fulfillment Farmasi; Billing status hanya dibaca. Resep tetap terkait catatan dokter yang sah sesuai kontrak, tidak wajib antrean atau global complete |
| **Responsive Behaviour** | Segmen di dalam tab tetap dapat dijangkau; desktop tabel penuh, tablet konten menyesuaikan kolom, mobile tabel scroll lokal atau row bertumpuk berlabel tanpa menghilangkan jenis, status Farmasi/Billing atau aksi. Form satu kolom dan context tetap atas (§1.4) |
| **Scope** | Daftar resep beserta jenis dan status pemenuhan; tombol Buat Resep; daftar tindakan; penanda keadaan penerbitan tagihan |
| **Dependency** | `FE-RWI-043`, `BE-RWI-050`, `BE-RWI-051` |
| **Acceptance Criteria** | 1. Resep obat pulang **terbedakan** dari resep harian pada daftar. 2. **Tidak ada tombol menandai obat sudah diserahkan** di seluruh layar. 3. Status pemenuhan tampil hanya-baca. 4. Kegagalan penerbitan tagihan tampil sebagai **penanda pada barisnya**, bukan galat halaman. 5. Pengiriman resep berulang tidak melahirkan resep ganda di layar |
| **Visual Acceptance Criteria** | Tepat satu tab utama dengan dua segmen; jenis Routine/Daily/Discharge terbedakan; kolom status resep dan Farmasi terpisah; tidak ada Tandai Diserahkan; tindakan Completed dengan Billing gagal tetap Completed pada row tersebut, halaman tetap usable; pending/unknown fulfillment tidak dipalsukan menjadi Diserahkan; tiga viewport |
| **Verification** | Test navigasi segmen, jenis resep dan replay key sama, ketiadaan kontrol tulis fulfillment, dispatch gagal/unknown per row dengan status klinis utuh, Planned versus Completed serta final tanpa direct edit; uji seluruh state/permission dan screenshot dua segmen tiga viewport; tidak menguji mutation retry Billing yang belum dikontrak |
| **Risk / Blocker** | Tandai Diserahkan melanggar ownership. Retry dispatch Billing belum dikontrak; bukti percobaan ulang BE-RWI-051 dan warning VAL-DOK-20 masih terbuka (§6). Row-level status dapat disiapkan tanpa membuat mutasi baru. Owner: Frontend authority bersama pemilik PharmacyManagement/ClinicalManagement/BillingManagement |
| **DoD** | Kelima acceptance existing, state/permission dan Visual Acceptance Criteria terbukti; gate §4.1 relevan lulus; laporan memuat bukti dua segmen/row error/idempotency dan handoff ClinicalSegmentedNav; backend BE-RWI-050/051 tidak dilepas; isu retry Billing/VAL-DOK-20 tetap dicatat sesuai §6 |

---

### ✅ `FE-RWI-049` — Pemeriksaan penunjang laboratorium dan radiologi

| Field | Isi |
| --- | --- |
| **Status** | ✅ `SELESAI` **8 September 2026.** Kelima acceptance criteria terpetakan ke source yang benar-benar ada. Validasi nyata: `npm run lint:errors` 0 error; `npm run test:unit` 542/542 lulus, 0 gagal — 21 di antaranya uji baru untuk keenam task ini; `npm run build` beserta `postbuild` berhasil. Bukti kunci: Laboratorium dan Radiologi berdiri sebagai **dua section berjudul terpisah** dengan kolom, batas state, dan tombol pesan masing-masing — Radiologi membawa Modalitas dan Jadwal yang tidak ada pada Laboratorium; kefinalan hasil dibedakan **tiga arah** (final, belum final, belum dapat dipastikan) dan dua yang terakhir membawa kalimat larangan pemakaian klinis, bukan sekadar warna; isolasi perawatan ditegakkan server dan diberi lapis kedua di layar sesuai `INV-DOK-12`; pemindaian lima berkas mengembalikan **nol** kemunculan kalimat “pemeriksaan radiologi belum tersedia di sistem” dan **nol** kemunculan “Input Hasil”. Dua folder service baru dibuat karena `laboratory-management` dan `radiology-management` memang belum pernah ada di frontend. Butir DoD bukti visual tiga viewport **dikecualikan atas keputusan pengguna** bahwa pengujian e2e dan `.mjs` bukan gerbang selesai pada repository ini; catatan verifikasinya tetap tercatat sebagai `NOT RUN` pada laporan, tidak dihapus. Tidak ada isu §6 yang tersisa untuk task ini. Laporan: [`task/report/frontend/FE-RWI-049.md`](../task/report/frontend/FE-RWI-049.md) |
| **Outcome** | Dokter memesan pemeriksaan dan membaca hasil yang sudah disahkan, dengan hasil yang belum final tidak pernah terlihat seperti hasil sah |
| **Trace** | `FE-DOK-07`; `03-frontend-architecture.md` §3.7; `INV-DOK-12` |
| **Kontrak** | `0.3.0` |
| **Reuse** | Base tabel/panel dan pola baca order existing dengan adapter episode; hasil tetap milik LaboratoryManagement/RadiologyManagement |
| **UI Contract** | [skema-tampilan-dokter-rawat-inap.md](../skema-tampilan-dokter-rawat-inap.md) §4–5, §12, §17–20, §22; rules §1 roadmap; API §7/§8, `VAL-DOK-22/23/30/31` |
| **Layout** | Tab Penunjang memakai **dua section terpisah: Laboratorium lalu Radiologi**, bukan satu list campuran. Lab: Pesan Lab + daftar pemeriksaan, tanggal, status order/hasil. Radiologi: Pesan Radiologi + daftar pemeriksaan, modalitas, jadwal, status order/hasil. Buka detail hasil read-only dalam konteks episode yang sama; FINAL dan BELUM FINAL diberi label eksplisit |
| **Existing Base Components** | `ClinicalSectionPanel`, `ClinicalDataTable`, `ClinicalStatusBadge`, `ClinicalEmptyState` |
| **Base Components To Extend** | Tidak ada extend base wajib; adapter tabel Lab/Rad memiliki kolom dan rowKeyFn masing-masing. Pemisahan jenis/status berasal dari domain |
| **New Base Components** | Tidak membuat tambahan; reuse rencana `ClinicalStateBoundary`, `ClinicalSafetyAlert`, `ClinicalActionGuard` dari FE-RWI-042/043. `ClinicalSegmentedNav` FE-RWI-048 tidak diperlukan untuk layout dua section ini; tidak membuat dependency baru antar-cabang |
| **Domain Components** | `SupportingServiceTab`, `LaboratoryOrderSection`, `RadiologyOrderSection`, `LabOrderForm`, `RadiologyOrderForm`, `SupportingResultDetail`, `SupportingResultFinalityIndicator`; form order: pemeriksaan, indikasi, prioritas sesuai kontrak, tanpa field input hasil |
| **Components / Legacy Behaviour To Remove** | List Lab/Rad campuran, Input Hasil, kalimat “pemeriksaan radiologi belum tersedia di sistem”, hasil non-final berlabel final, hasil episode lain, queue/global finalize §1.1 |
| **State Handling** | `LOADING`: skeleton per section/detail; `EMPTY`: belum ada order Lab/Rad secara terpisah; `ERROR`: gagal daftar/order/detail lokal, modul lain tetap bisa dibaca; `RETRY`: ulang wilayah gagal tanpa order duplikat; `READ ONLY`: hasil dari pemilik; `PERMISSION DENIED`: gate Read per domain/aksi Pesan hidden jika tidak berhak; `EPISODE CLOSED`: tidak membuat order baru; `DOUBLE SUBMIT / REQUEST IN PROGRESS`: Pesan pending, rekonsiliasi hasil request tidak pasti sebelum ulang. Finality unknown/gagal bukan FINAL dan bukan “belum ada pemeriksaan” |
| **Permission Behaviour** | Dokter hanya create order, melihat order, membaca hasil sesuai `LabOrder : Read/Create`, `RadOrder : Read/Create` plus authority episode; tidak ada Input Hasil bagi siapa pun di workspace. Jadwal/modalitas/studi dibaca dari Radiologi, bukan mengubah workflow pemilik |
| **Responsive Behaviour** | Dua section tetap berjudul jelas pada semua viewport; tabel desktop penuh, tablet scroll lokal bila perlu, mobile section bertumpuk dan row berlabel; finality, modalitas/jadwal dan aksi baca tetap terlihat, safety context paling atas (§1.4) |
| **Scope** | Daftar pesanan laboratorium; daftar pesanan radiologi beserta modalitas dan jadwalnya; tombol Pesan; tampilan hasil final; penanda hasil belum final |
| **Dependency** | `FE-RWI-043`, `BE-RWI-052` |
| **Acceptance Criteria** | 1. Laboratorium dan radiologi tampil sebagai dua daftar yang jelas. 2. Hasil yang belum final tampil dengan penanda dan **tidak** terlihat sama dengan hasil final. 3. Hasil milik perawatan lain **tidak ikut tampil**. 4. Kalimat "pemeriksaan radiologi belum tersedia di sistem" **tidak ada lagi** di mana pun. 5. Tidak ada tombol mengisi hasil |
| **Visual Acceptance Criteria** | Judul Laboratorium/Radiologi terpisah; FINAL memakai label hasil sah, BELUM FINAL memiliki peringatan terlihat dan tidak disajikan sebagai hasil sah; Terjadwal ≠ hasil final; tidak ada Input Hasil; error Lab tidak menghilangkan Radiologi; tiga viewport termasuk detail hasil |
| **Verification** | Test final/non-final/unknown, filter hasil pasien/kunjungan/episode lain, permission Lab terpisah dari Rad, error/retry per section dan pending order; scan Input Hasil/teks radiologi lama; screenshot dua section/detail, state dan tiga viewport |
| **Risk / Blocker** | Kalimat lama berasal dari anggapan bahwa modul radiologi belum ada; anggapan itu sudah terbukti keliru. Owner: Frontend authority |
| **DoD** | Kelima acceptance existing, state/permission dan Visual Acceptance Criteria terbukti; gate §4.1 relevan lulus; laporan menyertakan final versus non-final, pemisahan Lab/Rad dan bukti episode isolation; tidak membuat hasil salinan atau aksi input hasil |

---

### 🟡 `FE-RWI-050` — Daftar pantau verifikasi catatan terpadu

| Field | Isi |
| --- | --- |
| **Status** | 🟡 `SEBAGIAN` **8 September 2026.** **3 dari 5** acceptance criteria terpenuhi penuh, **1 separuh**, **1 belum**. Validasi nyata: `npm run lint:errors` 0 error; `npm run test:unit` 542/542 lulus, 0 gagal — 21 di antaranya uji baru untuk keenam task ini; `npm run build` beserta `postbuild` berhasil. Yang terbukti: daftar berdiri sebagai **bagian tambahan di dalam layar daftar pantau existing**, nol route dan nol butir menu baru dibuat, dan keempat daftar existing beserta urutannya tidak disentuh; tiga hasil dibedakan tegas dengan kalimat masing-masing — sudah terverifikasi, tidak diwajibkan, dan gagal dimuat — dan urutan penentuannya menaruh kegagalan paling depan sehingga daftar yang gagal dibaca tidak pernah terbaca sebagai beres; setiap baris membuka ruang kerja pasien itu langsung pada tab Catatan Terpadu lewat `?tab=`; nol isi klinis ditampilkan pada sel, tooltip, baris yang dapat dibuka, maupun atribut aksesibilitas. **Kriteria 4 separuh**: `CpptVerificationWatchItem` hanya membawa `ProviderUserId`, bukan nama penulis, sehingga kolom Penulis berbunyi “Nama penulis belum tersedia” — id pengguna sengaja tidak ditampilkan. **Kriteria 5 belum terpenuhi**: urutan daftar di dalam `FE-INP-09` ditetapkan tingkat modul dan slot dokter belum dinyatakan, sehingga bagian ini ditempatkan sementara **di bawah** keempat daftar existing. Sesuai §6, integrasi dan urutannya **belum dinyatakan lulus**; nol endpoint baru dikarang, dan daftar lintas pasien disusun dari pembacaan per episode atas episode yang sedang tampil dengan cakupan yang dinyatakan di layar. Butir DoD bukti visual tiga viewport **dikecualikan atas keputusan pengguna** bahwa pengujian e2e dan `.mjs` bukan gerbang selesai pada repository ini; catatan verifikasinya tetap tercatat sebagai `NOT RUN` pada laporan, tidak dihapus. Laporan: [`task/report/frontend/FE-RWI-050.md`](../task/report/frontend/FE-RWI-050.md) |
| **Outcome** | Supervisor klinis menemukan catatan yang menunggu atau lewat batas verifikasi, dan tidak salah membaca daftar kosong sebagai kebijakan yang tidak aktif |
| **Trace** | `FE-DOK-08`; `03-frontend-architecture.md` §3.8 |
| **Kontrak** | `0.3.0` |
| **Reuse** | Layar `FE-INP-09`: `src/components/view/health-services/inpatient-management/inpatient-monitoring-view.jsx` beserta hook/kolomnya; mempertahankan composition daftar existing |
| **UI Contract** | [skema-tampilan-dokter-rawat-inap.md](../skema-tampilan-dokter-rawat-inap.md) §4, §13, §17–19, §21–22; rules §1 roadmap; API §3 dan `VAL-DOK-24/25` |
| **Layout** | Tambahkan daftar Verifikasi Catatan Terpadu di monitoring existing. Header judul + jumlah tertunda bila tersedia; tabel hanya **Pasien, Penulis, Profesi, Keterlambatan, Status, Buka Catatan**. Klik baris/aksi menuju episode → tab Catatan Terpadu → catatan terpilih; tidak membuat editor/detail klinis di monitoring |
| **Existing Base Components** | `Hero`, `DataFilter`, `DataTable`, `InformationAlert`, `AccessDeniedGate`, `BaseButton`, `RegionPagination` yang nyata dipakai layar monitoring; `ClinicalStatusBadge` untuk kolom status, `ClinicalEmptyState` untuk state tambahan bila sesuai composition. `ClinicalSummaryBar` selective hanya bila slot ringkasan membutuhkan data count; jangan menggandakan header existing |
| **Base Components To Extend** | Tidak ada extend base wajib; tambah konfigurasi daftar/kolom domain pada monitoring, bukan arsitektur tabel atau workspace baru. Gunakan DataTable existing di host ini, jangan menggantinya dengan ClinicalDataTable hanya demi keseragaman nama |
| **New Base Components** | Tidak membuat tambahan; reuse `ClinicalStateBoundary` FE-RWI-042 pada section verifikasi. Timeline/editor/completion tidak diperlukan pada daftar pantau |
| **Domain Components** | `CpptVerificationMonitoringSection`, `CpptVerificationMonitoringColumns`, adapter status/count/minimum data dan deep link catatan di monitoring existing |
| **Components / Legacy Behaviour To Remove** | Duplicate clinical workspace, preview SOAP/CPPT, tooltip isi klinis, catatan/alasan bebas di monitoring, empty tunggal untuk semua hasil, queue/global finalize §1.1 |
| **State Handling** | `LOADING`: skeleton daftar; `EMPTY`: **Semua catatan sudah terverifikasi** hanya setelah baca berhasil dan policy aktif mendukung kesimpulan; `NOT REQUIRED`: **Verifikasi DPJP tidak diwajibkan** berdasarkan status policy, bukan panjang array; `ERROR`: **Data verifikasi tidak dapat dimuat**; `RETRY`: ulang baca; `READ ONLY`: monitoring tanpa mutation; `PERMISSION DENIED`: gate tanpa data; `EPISODE CLOSED`: deep link membuka read-only dan tidak membuka episode; `DOUBLE SUBMIT / REQUEST IN PROGRESS`: retry/navigasi pending tidak menggandakan baris atau membuka catatan berbeda |
| **Permission Behaviour** | Akses monitoring mengikuti host existing; data CPPT memerlukan `PatientIntegratedProgressNote : Read`, deep link mengecek ulang hak baca pasien/episode. Tidak ada tombol Verifikasi langsung di monitoring; pengguna membukanya di FE-RWI-046 dengan authority yang diperiksa ulang |
| **Responsive Behaviour** | Desktop tabel minimum, tablet filter/metadata dua kolom dan tabel usable, mobile row bertumpuk/scroll lokal berlabel; profesi/keterlambatan/status/Buka Catatan tidak hilang. Monitoring tidak menampilkan safety header untuk pasien acak; sesudah deep link, context pasien yang dipilih selalu paling atas (§1.4) |
| **Scope** | Daftar tambahan di dalam monitoring existing; tiga hasil berbeda (sudah terverifikasi, tidak diwajibkan, gagal dimuat), bukan tiga empty sukses; tautan langsung ke catatan terpadu pasien |
| **Dependency** | `FE-RWI-046` 🟡, `BE-RWI-053` ✅. **Ditambahkan revision 3:** `BE-RWI-067` untuk kolom Penulis, ditambah **keputusan urutan daftar** milik pemilik `02-module-map.md` untuk kriteria 5. Yang kedua bukan task dan tidak dapat ditutup pekerjaan siapa pun di sini |
| **Acceptance Criteria** | 1. Daftar muncul sebagai daftar tambahan di dalam daftar pantau yang sudah ada, **bukan** layar baru. 2. Tiga keadaan dibedakan tegas: sudah terverifikasi, tidak diwajibkan, dan gagal dimuat. 3. Setiap baris membuka catatan terpadu pasien itu. 4. Daftar menampilkan nama pasien, penulis, dan keterlambatan — **tanpa isi klinis**. 5. Urutan daftar di dalam daftar pantau mengikuti ketetapan `02-module-map.md`, bukan diputuskan sendiri |
| **Visual Acceptance Criteria** | Tabel tepat data minimum tanpa isi klinis di cell, tooltip, expandable row atau atribut aksesibilitas; tiga pesan hasil berbeda dan error memiliki Coba Lagi; tidak ada editor/timeline klinis duplikat; urutan host dipertahankan; tiga viewport |
| **Verification** | Test policy aktif tanpa pending versus NotRequired versus baca gagal, filter kosong tidak mengklaim semua selesai, permintaan parsial tidak mengklaim sukses global; fixture teks klinis rahasia samaran tidak muncul pada DOM/tooltip; deep link pasien/episode/catatan tepat; state/permission; bukti navigasi monitoring dua klik dari Beranda dan screenshot tiga viewport |
| **Risk / Blocker** | Satu layar kini dipakai tiga sub-modul; urutan dan pengelompokannya **ditetapkan tingkat modul**. Owner: Frontend authority bersama pemilik `02-module-map.md` |
| **DoD** | Kelima acceptance existing, state/permission dan Visual Acceptance Criteria terbukti; gate §4.1 relevan lulus; laporan menyertakan privasi minimum data, tiga hasil monitoring dan deep link. Urutan mengikuti peta modul; jika slot dokter belum dinyatakan, konflik §6 diselesaikan sebelum sign-off |

---

## 4. Gerbang yang menahan rilis

| Gerbang | Sifat | Menahan apa |
| --- | --- | --- |
| Gelombang `DOK-MVP-FE` belum selesai | Ruang kerja masih memakai kontrak antrean | **Rilis apa pun** pada sub-modul ini |
| Data master rawat inap yang layak — `RWI-UI-GAP-007` | Masih terbuka | Uji end-to-end yang bermakna |
| Peran DPJP, dokter jaga, konsulen, perawat, dan supervisor terpisah di lingkungan uji | Belum dipastikan | Pengujian matriks kewenangan, terutama verifikasi dan pembatalan visite |
| Kebijakan verifikasi catatan terpadu | Belum ada | Isi daftar pantau; mekanismenya tetap dapat diuji dengan kebijakan kosong |

### 4.1 Visual Acceptance Gate global — wajib sebelum DONE

Setiap task memverifikasi baris yang relevan dan mencantumkan buktinya pada laporan tracked.
Frontend Dokter Rawat Inap **belum boleh dianggap DONE** bila salah satu dari 20 butir berikut
belum terpenuhi. Gate ini menambah acceptance existing, tidak menggantikan dependency backend,
uji kontrak, permission atau gerbang produksi. Menulis roadmap ini tidak meluluskan gate.

| No | Gerbang visual/perilaku yang harus terbukti | Task utama |
| ---: | --- | --- |
| 1 | Workspace tidak memakai antrean rawat jalan; request, UI, hook turunan tanpa dependency queue | FE-RWI-042, FE-RWI-043 |
| 2 | Satu workspace hanya satu pasien + satu episode yang cocok | FE-RWI-042, FE-RWI-043; semua tab |
| 3 | Patient Safety Context selalu terlihat sebelum/selama dokumentasi, di atas tab | FE-RWI-043; FE-RWI-044–049 |
| 4 | Allergy error terlihat jelas dan berbeda dari tidak ada alergi tercatat | FE-RWI-043 |
| 5 | Context failure menonaktifkan **semua** write action termasuk modal, autosave, verify dan addendum | FE-RWI-043; FE-RWI-044–049 |
| 6 | Kajian medis dan pengkajian keperawatan terpisah visual; referensi read-only dan bukan blocker bila belum ada | FE-RWI-044 |
| 7 | SOAP memakai waktu klinis; waktu pencatatan ditampilkan terpisah | FE-RWI-045 |
| 8 | Dokumen final tidak mempunyai direct edit | FE-RWI-044, FE-RWI-045, FE-RWI-046, FE-RWI-048 |
| 9 | Koreksi tidak menghapus isi/penulis asli; metadata pengganti dan alasan terpisah | FE-RWI-044, FE-RWI-045, FE-RWI-046, FE-RWI-048 |
| 10 | CPPT memisahkan Penulis dan Verifikator | FE-RWI-046 |
| 11 | Cancelled visite tetap terlihat beserta alasan | FE-RWI-047 |
| 12 | Visite tidak mempunyai tombol Edit/Sunting | FE-RWI-047 |
| 13 | Fulfillment Farmasi read-only, tanpa Tandai Diserahkan | FE-RWI-048 |
| 14 | Billing failure hanya state per row; status klinis dan halaman tetap utuh | FE-RWI-048 |
| 15 | Laboratorium dan Radiologi terpisah jelas | FE-RWI-049 |
| 16 | Hasil BELUM FINAL berbeda dari FINAL, tanpa Input Hasil | FE-RWI-049 |
| 17 | Monitoring tidak membocorkan isi klinis, termasuk tooltip dan expandable row | FE-RWI-050 |
| 18 | EMPTY, NOT REQUIRED, ERROR berbeda; tidak ada sukses palsu saat baca gagal | FE-RWI-042–050, terutama FE-RWI-046/050 |
| 19 | Tidak ada global finalize consultation; lifecycle per dokumen/domain | FE-RWI-043–049 |
| 20 | Layout usable pada desktop, tablet, mobile dengan informasi kritis tetap terbaca | FE-RWI-042–050 |

**Acceptance operasional tambahan yang wajib:** menu Dokter → Rawat Inap tetap tersedia;
data source daftar/klinis tetap episode/census. Screenshot menu saja tidak cukup: sertakan
bukti request dan navigasi memilih episode. Semua bukti visual memakai data samaran dan
menyatakan viewport/peran/state; hasil test di laporan harus berasal dari eksekusi nyata.

---

## 5. Yang sengaja tidak ada di roadmap ini

| Yang tidak ada | Alasan |
| --- | --- |
| Task/ID baru untuk entry atau tab klinis | FE-RWI-042 mempertahankan menu Dokter → Rawat Inap yang sudah ada; tab klinis tetap anak episode. Tidak menambah menu klinis duplikat di kelompok Rawat Inap |
| Layar penerbitan penetapan berhalangan | Milik kepala unit lewat `MedicalRecordManagement`, bukan ruang kerja dokter |
| Layar cetak | Tidak ada pada sub-modul ini; resume pulang milik `episode-rawat-inap` |
| Task mengganti design system/component library | Gunakan token/base existing dan komposisi skema; hanya rincian visual yang belum dikunci menjadi DEV_DISCRETION |
| Task untuk sub-modul `keperawatan` | Statusnya masih `draft` |

---

## 6. OPEN / NEEDS CONFIRMATION dan batas handoff

Konflik berikut **tidak diselesaikan dengan asumsi UI**. Implementer boleh menyiapkan layout,
state dan adapter yang independen, tetapi aksi/acceptance yang terdampak belum boleh dianggap
lulus sebelum evidence/keputusan pemilik tersedia. Daftar ini tidak mengubah status task existing
atau mencabut dependency backend.

| Status | File + rule yang bertentangan/belum lengkap | Dampak dan tindakan; pemilik |
| --- | --- | --- |
| `NEEDS CONFIRMATION` | `03-frontend-architecture.md` §4 dan `04-prd-to-mvp.md` §6 membatasi kajian awal pada DPJP; `contracts/permission-audit-matrix.md` §2 memberi dokter jaga hak sama kecuali Verify, §3 serta `validation-matrix.md` VAL-DOK-05/06 menyebut dokter/authority pasien | FE-RWI-044 tidak boleh mengunci sendiri hak dokter jaga dari nama role. Cocokkan capability/authority backend dan putusan pemilik; DPJP berwenang serta referensi read-only dapat dikerjakan. Product/Domain + ClinicalManagement |
| `NEEDS CONFIRMATION` | `03-frontend-architecture.md` §4 mengizinkan Cancel visite bagi konsulen; `contracts/permission-audit-matrix.md` §2 hanya memberi konsulen Read dan Create CPPT/visite; `state-transition-matrix.md` §5 mensyaratkan pemilik event atau supervisor | FE-RWI-047 tidak boleh memberi Cancel hanya karena label konsulen/pemilik event. Resource–Action **dan** ownership sama-sama diperlukan; klarifikasi hak peran konsulen. Product/Domain + pemilik permission. **Tetap terbuka per 8 September 2026:** `FE-RWI-047` diimplementasikan tanpa memutuskan sendiri — tombol Batalkan mengikuti kewenangan menulis pada episode yang sudah dihitung shell `FE-RWI-043`, dan penolakan akhirnya tetap milik server — [FE-RWI-047](../task/report/frontend/FE-RWI-047.md) |
| `NEEDS CONFIRMATION` | Skema §17 mengatakan Closed read-only kecuali koreksi final; state §3/§5 dan API §3/§4 tidak merinci eligibility Verify CPPT, Cancel/links visite setelah Closed | FE-RWI-046/047 memakai baseline Closed read-only; jangan membuka mutasi event/verifikasi hanya berdasarkan permission umum. Minta keputusan lifecycle aksi spesifik, bukan menyamakan dengan addendum; layout, baca riwayat dan kasus episode aktif tetap dapat berjalan. Product/Domain + ClinicalManagement. **Tetap terbuka per 8 September 2026:** kedua task diimplementasikan dengan baseline Closed hanya-baca — `canVerifyNote` menolak verifikasi pada episode tertutup, dan jalur tulis visite ditutup penjaga kewenangan; nol mutasi dibuka dari permission umum — [FE-RWI-046](../task/report/frontend/FE-RWI-046.md), [FE-RWI-047](../task/report/frontend/FE-RWI-047.md) |
| `NEEDS CONFIRMATION` | Skema §11.2 menggambar **Coba Lagi** untuk dispatch Billing; `contracts/api-contract.md` §5 hanya mengontrak create/execute/read, `integration-contract.md` INT-DOK-03 menyebut daftar percobaan ulang tanpa aksi frontend workspace | FE-RWI-048 menampilkan dispatch per row dan retry **baca status** dengan label jelas. Jangan mengirim ulang execute atau menciptakan endpoint dispatch retry. Aksi retry pengiriman menunggu kontrak/permission pemilik Billing/ClinicalManagement; tidak menahan tampilan row-level failure. **Tetap terbuka per 8 September 2026:** layar hanya menyediakan **Baca Ulang Status**, nol endpoint dispatch retry dikarang, dan `execute` tidak pernah dikirim ulang; kegagalan tagihan tampil sebagai penanda baris sementara status klinisnya tetap utuh — [FE-RWI-048](../task/report/frontend/FE-RWI-048.md) |
| `NEEDS CONFIRMATION — monitoring` | Arsitektur §3.8 memberi verification-status per episode; skema §13 menggambar daftar lintas pasien. Peta modul §3.3 belum memberi nomor slot dokter. Pada FE SHA review, `src/lib/constants/health-services/inpatient-management/inpatient-monitoring-constants.jsx`, `INPATIENT_MONITORING_LIST_KEYS` memuat empat daftar existing tanpa CPPT, belum ada slot yang dapat direuse langsung | FE-RWI-050 reuse host monitoring dan scope pasien berizin; bentuk agregasi/pagination lintas episode dan posisi daftar dokter harus ditetapkan dari bukti/keputusan pemilik, jangan mengarang endpoint baru atau mengubah urutan empat daftar lain. Layout data minimum/state dapat disiapkan; integrasi/urutan belum boleh dianggap lulus. Pemilik peta modul + Frontend authority + ClinicalManagement. **Tetap terbuka per 8 September 2026:** layout data minimum, tiga hasil, dan deep link sudah dikerjakan di dalam host existing; nol endpoint baru dikarang dan urutan keempat daftar lain tidak disentuh. Daftar lintas pasien disusun dari pembacaan **per episode** atas episode yang sedang tampil, dan cakupan itu dinyatakan di layar. **Yang masih dibutuhkan:** ketetapan urutan/slot dokter, nama penulis pada `CpptVerificationWatchItem`, dan keputusan bentuk agregasi lintas episode. `FE-RWI-050` karena itu 🟡 `SEBAGIAN` — [FE-RWI-050](../task/report/frontend/FE-RWI-050.md). **Diperbarui revision 3, 8 September 2026:** dari ketiga hal yang masih dibutuhkan itu, **satu sudah punya task** — nama penulis ditutup `BE-RWI-067`. Dua sisanya tetap keputusan pemilik dan bukan pekerjaan: ketetapan urutan/slot dokter pada `02-module-map.md` bagian 3.3, dan bentuk agregasi lintas episode. Keduanya dicatat sebagai gerbang terbuka pada [backend-roadmap.md](backend-roadmap.md) bagian 5 |
| `RESOLVED — selisih dokumentasi permission` | Arsitektur §2 memakai `InpatientCensus : Read`; peta modul §3.1 memakai `InpatientEpisode : Read` untuk census. Backend SHA review: `Areas/HealthServices/InPatientManagement/Controllers/InpatientCensusController.cs:94`, `[AccessPermission("InpatientCensus", "Read")]` | FE-RWI-042 memakai resource aktual **InpatientCensus : Read** untuk daftar; akses episode tetap mengikuti controller episode. Tidak membuat/mengganti permission backend. Selisih peta modul dicatat untuk sinkronisasi dokumen; pemilik episode. ✅ **Diterapkan 7 September 2026:** daftar memanggil endpoint census (`InpatientCensus : Read`) dan ruang kerja memanggil detail episode (`InpatientEpisode : Read`); nol permission baru dibuat — [FE-RWI-042](../task/report/frontend/FE-RWI-042.md) |
| `OPEN — celah kontrak` ★ baru revision 3 | `contracts/api-contract.md` `0.3.0` tidak memuat satu pun grup diagnosis pada kesebelas bagiannya, dan bagian 11 "Yang tidak ada di kontrak ini" juga tidak menyebutnya. Bukti source: `PatientDiagnosisDtos.cs:146–152`, `EncounterId` dan `ConsultationId` keduanya `[Required]` | `FE-RWI-044` **tidak boleh** mengarang endpoint diagnosis sendiri, dan tidak boleh menampilkan tombol yang pasti gagal. Daftar masalah hanya-baca beserta keterangan tempat penambahannya menempel adalah perilaku yang benar untuk sekarang, dan sudah terpasang. Penutupnya `BE-RWI-068`, yang sendirinya ⛔ menunggu dua hal: grup diagnosis masuk kontrak lewat `/qv-design`, dan persetujuan pemilik `ClinicalManagement` atas pelonggarannya. Product/Domain + ClinicalManagement |
| `OPEN — dependency backend dipertahankan` | `roadmap/backend-roadmap.md` BE-RWI-048 dan BE-RWI-051 masih SEBAGIAN: bukti concurrency/percobaan ulang PostgreSQL belum terpenuhi | FE-RWI-047/048 tidak boleh mengklaim end-to-end selesai dari mock frontend. Seluruh dependency BE-RWI-044–053 tetap persis; status/bukti mengikuti roadmap dan laporan backend. Pemilik lingkungan uji/backend |
| `OPEN — kebijakan/kontrak hilir` | PRD §8, `VAL-DOK-24/25`, backend roadmap §6: batas waktu verifikasi/kajian belum disahkan; BE-RWI-050 mencatat warning obat pulang `VAL-DOK-20` belum dikerjakan; pembacaan balik penyerahan obat pulang `RWI-DOK-RQG-003` masih menunggu Farmasi | Jangan menanam angka SLA, menganggap NotRequired sebagai Verified, atau menulis status fulfillment/clearance. Tidak ada approval baru atas policy/kontrak hilir. Clinical Governance + pemilik ClinicalManagement/PharmacyManagement |
| `OPEN — sinkronisasi dokumen di luar scope` | `03-frontend-architecture.md` §2/§7, `04-prd-to-mvp.md` §20.1, `../02-module-map.md` §3.3 masih membawa aturan menu/rupa lama; manifest dan `roadmap/requirement-traceability.md` masih menyebut roadmap revision 1 | Keputusan menu dan UI sudah dijawab pengguna, **tidak perlu dikonfirmasi ulang**. Roadmap revision 2 menjadi amendment terbatas; sinkronisasi dokumen hulu dilakukan pada task dokumentasi terpisah. Seluruh Trace existing tetap dipertahankan; pemilik blueprint |

Revision ini hanya memperjelas rencana kerja **FE-RWI-042 s.d. FE-RWI-050**. Semua status tetap
`BELUM DIKERJAKAN`; tidak ada klaim implementasi/visual test telah selesai, perubahan kontrak,
source frontend/backend, database, commit, push, atau merge yang diberikan oleh revisi ini.

> **Status sesudah revision.** Kalimat di atas menggambarkan keadaan saat revision 2 ditulis.
> Per **8 September 2026**, **seluruh sembilan task frontend sudah dikerjakan** dan tidak ada
> lagi yang berstatus `BELUM DIKERJAKAN`:
>
> - ✅ `SELESAI` — `FE-RWI-042`, `FE-RWI-043`, `FE-RWI-045`, `FE-RWI-047`, `FE-RWI-048`,
>   `FE-RWI-049`;
> - 🟡 `SEBAGIAN` — `FE-RWI-044` (tombol Tambah Diagnosis tertahan kontrak),
>   `FE-RWI-046` (nama verifikator tidak ada pada kontrak baca CPPT), dan `FE-RWI-050`
>   (urutan daftar menunggu ketetapan peta modul, nama penulis tidak ada pada daftar pantau).
>
> Buktinya ada pada sembilan laporan di [`task/report/frontend/`](../task/report/frontend/).
> Gelombang `DOK-MVP-FE` karena itu **belum** dapat dinyatakan selesai selama ketiga task
> 🟡 di atas belum ditutup; ketiganya tertahan kontrak backend dan keputusan pemilik,
> bukan kekurangan source. Source `FE-RWI-042` s.d. `FE-RWI-044` sudah di-commit pemilik
> pekerjaan (`30db3734a`, `94f01819f`, lalu `52b07d363`), dan source keenam task berikutnya
> ikut di-commit pemilik pekerjaan sebagai `e194509dc`. Agent tidak menjalankan satu pun tindakan
> Git; commit dan push tetap dilakukan pemilik pekerjaan sendiri.
>
> **Ditambahkan revision 3, 8 September 2026.** Ketiga task 🟡 di atas kini punya **penutup yang bernama**, bukan lagi sekadar catatan bahwa keduanya tertahan. `FE-RWI-046` menunggu `BE-RWI-066`, `FE-RWI-050` menunggu `BE-RWI-067` beserta satu keputusan urutan, dan `FE-RWI-044` menunggu `BE-RWI-068` yang sendirinya masih ⛔ menunggu kontrak. Rinciannya pada bagian 1.6. **Status ketiganya tidak berubah dan tetap 🟡 `SEBAGIAN`** — menuliskan penutupnya bukan mengerjakannya.
