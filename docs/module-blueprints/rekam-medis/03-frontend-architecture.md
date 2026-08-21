# Frontend Architecture — Rekam Medis: Existing Clinical Foundation

| Field | Nilai |
| --- | --- |
| Status | `draft` |
| Scope | Memakai workspace dokter dan tab klinis existing; tidak membuat menu medis yang belum ada |
| Input | Arsitektur domain lengkap revision `1` |
| Production gate | `RM-APR-002`; development-draft diizinkan oleh `RM-APR-006` |

## Kontrak Fungsional

Frontend tidak menjadi owner pasien, encounter, tenaga kesehatan, lokasi, fakta klinis, hasil
penunjang, atau resep. Komponen yang kelak menampilkan data Rekam Medis wajib memakai ID dan response
dari service owner. Revision ini memakai route doctor queue existing dan tab SOAP, CPPT, resep,
tindakan, surat, assessment, serta vital preview yang sudah reachable. Tidak ada sidebar/menu baru.

## Existing Workspace yang Dipakai

| Area existing | Lokasi frontend | Keputusan |
| --- | --- | --- |
| Doctor queue page | `src/app/health-services/registration-management/doctor-queues/page.jsx` | Reuse route existing. |
| Doctor queue view | `src/components/view/health-services/registration-management/doctor-queues/doctor-queue-view.jsx` | Extend tanpa membuat workspace kedua. |
| Consultation tabs | `src/components/features/health-services/doctor-queue-features/ConsultationTabs.jsx` | Reuse hierarchy existing. |
| SOAP tab | `.../doctor-queues/tabs/soap/doctor-soap-tab.jsx` | Extend state signed/read-only kelak. |
| CPPT tab | `.../doctor-queues/tabs/cppt/doctor-cppt-tab.jsx` | Extend warning finality/correction; jangan pakai delete untuk signed record. |
| Procedure tab | `.../doctor-queues/tabs/procedure/doctor-procedure-tab.jsx` | Reuse. |
| Prescription tab | `.../doctor-queues/tabs/prescription/doctor-prescription-tab.jsx` | Reuse owner Pharmacy. |
| Finalization panel/modal | `src/components/features/health-services/doctor-queue-features/FinalizeConsultationPanel.jsx` dan `FinalizeConsultationModal.jsx` | Repair mismatch checklist dan multi-request pada task terpisah. |

Menu break-glass, release, worklist Unit RM, IGD terpadu, dan rawat inap terpadu belum dibuat pada
revision ini karena user meminta existing-first.

## Data yang Boleh Dikonsumsi

| Data | Owner | Penggunaan frontend kelak | Larangan |
| --- | --- | --- | --- |
| Patient reference | Patient Management | Menampilkan identitas sesuai izin. | Membuat patient state sebagai sumber kebenaran tandingan. |
| Encounter reference | Registration | Memilih konteks episode pelayanan. | Mengubah status encounter dari layar RM. |
| Workforce reference | Workforce/Identity | Menampilkan pembuat/profesi. | Menebak penugasan aktif dari nama dokter. |
| Location reference | Master Data/Registration | Menampilkan unit/klinik/ruang. | Menyimpan master lokasi RM. |
| Clinical fact reference | Clinical Management | Menampilkan fakta owner. | Mengedit fakta final melalui salinan UI. |

## Aksi per Peran

Tidak ada role atau permission baru pada revision ini. UI hanya menampilkan aksi existing bila API
menyatakan permission sesuai. Target contextual authorization tetap wajib sebelum akses RM dianggap
final.

| Peran/konteks existing | Aksi yang dipakai ulang | Batas |
| --- | --- | --- |
| Dokter pada doctor queue | Membaca assessment/vital; mengisi SOAP, diagnosis, tindakan, resep; menjalankan finalization validation. | Harus memiliki antrean/penugasan relevan; status completed kelak read-only. |
| Tenaga klinis pembuat CPPT | Membuat CPPT sesuai profesi. | Tidak boleh mengubah/menghapus record final. |
| Verifier/approver existing | Verify/approve dokumen atau consent sesuai permission source. | Permission generic belum cukup untuk authority RM final. |
| Pengguna tanpa hubungan pelayanan | Tidak ada aksi klinis. | Permission `Read` saja tidak boleh membuka data pasien. |

Menu Unit RM, break-glass, release, IGD terpadu, dan rawat inap terpadu tidak ditampilkan.

## State dan Failure Handling

| Keadaan | Perilaku wajib kelak |
| --- | --- |
| Loading | Tampilkan bahwa reference sedang dimuat; jangan tampilkan data pasien sebelumnya. |
| Empty | Bedakan “owner mengembalikan kosong” dari “owner gagal dihubungi”. |
| Error/timeout | Tampilkan kegagalan verifikasi sumber dan sediakan retry aman. |
| Patient–encounter mismatch | Hentikan tampilan fakta klinis dan tampilkan pesan kesalahan yang jelas. |
| Stale reference | Tampilkan provenance/waktu data bila kontrak kelak memakai cache. |
| Duplicate submit | Nonaktifkan tombol saat request berjalan; retry kelak memakai idempotency key. |
| Consultation stale | Respons `409` dari `ExpectedUpdatedAt` meminta pengguna memuat ulang tanpa menimpa perubahan lain. |
| Completed/final | Form menjadi read-only; selama correction belum tersedia, UI tidak menawarkan edit/delete. |
| Validation warning | Tampilkan item per bagian; bedakan error dan warning; acknowledgement hanya untuk warning yang diizinkan contract existing. |

**Contoh pesan:** “Encounter tidak sesuai dengan pasien yang dipilih. Data klinis tidak dibuka.”

## Hierarki Kewenangan UI

| Area | Authority | Status |
| --- | --- | --- |
| Security, privacy, ownership, dan patient–encounter consistency | Owner terkait | Wajib; bukan kewenangan developer |
| Alur utama dan warning klinis | Product/UI brief approved | Belum tersedia |
| Pola loading/error/empty | Project convention | Boleh dipakai selama tidak menyembunyikan kegagalan verifikasi |
| Detail visual | Developer | `DEV_DISCRETION` sesuai `RM-UI-001-B` |

## Cache dan Invalidation

Gunakan mekanisme fetch/state existing pada doctor workspace; revision ini tidak menambah cache
klinis baru. Setelah mutation berhasil, muat ulang consultation, diagnosis, procedure, CPPT,
document, consent, dan finalization validation terkait. Cache key—bila sudah ada—wajib menyertakan
patient, encounter, queue, dan owner record ID agar data pasien tidak tertukar.

## Accessibility dan Responsive Behavior

Gunakan pola responsive existing. Status read-only, error mismatch, warning finalisasi, dan alert
keselamatan harus memiliki teks atau ikon yang dapat dibaca pembaca layar dan tidak hanya dibedakan
melalui warna. Setelah finalisasi ditolak, fokus berpindah ke ringkasan error pertama.

## Test Dependency

- Unit test mapper/reference bila adapter frontend kelak dibuat.
- Integration test bahwa patient–encounter mismatch tidak membuka fakta klinis.
- Test bahwa error owner tidak dirender sebagai empty clinical record.
- Test finalization validation, stale `409`, tombol submit ganda, dan form read-only setelah complete.
- Regression test bahwa route/tab existing tetap bekerja dan menu baru tidak muncul.

Dokumen ini cukup sebagai desain draft existing-first, tetapi belum memberi task implementasi.
Kontrak task, acceptance criteria, dan approval delivery dibuat pada tahap perencanaan berikutnya.
