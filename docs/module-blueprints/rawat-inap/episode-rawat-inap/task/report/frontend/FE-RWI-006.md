# FE-RWI-006 — Petugas dapat membuka admisi beserta catatan awal isolasi

- TASK ID: `FE-RWI-006`
- TASK TYPE: Implementasi layar frontend
- COMPLEXITY: `HEAVY`
- CLASSIFICATION SCORE: 10 — dua repository 2; lebih dari 20 berkas diperiksa 2; lebih dari delapan berkas diubah 2; logika moderat 1; mengonsumsi kontrak yang sudah ada 1; database 0; menyentuh penjagaan akses tanpa mengubahnya 1; satu alur berbatas 1
- MODEL: Claude Opus 5
- TASK MODE: `FRONTEND`
- WRITE TARGET: `QuilvianSystemFrontendDev` pada branch `HamzahV2`. Backend hanya dibaca
- TASK/CONTRACT VERSION: roadmap frontend revision `2`; api contract `0.4.0` — `POST /episodes` dan `PATCH /episodes/{id}/isolation-requirement` berstatus ✅ **Tersedia**
- FILES INSPECTED: roadmap `FE-RWI-006`; api contract bagian Inpatient Episode; `InpatientEpisodeDtos.cs` (`OpenAdmissionRequest`), `InpatientEpisodeAssignmentDtos.cs` (`SetIsolationRequirementRequest`); `PatientController.cs` bagian `options` dan `PatientOptionResponse`; registry select; `BaseEditorForm`, `BaseCheckboxField`, `BaseTextAreaField`; komponen papan tempat tidur `FE-RWI-005`
- FILES CHANGED: `src/lib/constants/health-services/inpatient-management/inpatient-admission-constants.jsx` (baru); `src/utils/health-services/inpatient-management/inpatient-admission-utils.jsx` (baru); `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission.jsx` (baru); `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx` (baru); `src/app/health-services/inpatient-management/admissions/page.jsx` (baru); `tests/unit/inpatient-admission.test.mjs` (baru); `tests/e2e/inpatient-admission.spec.mjs` (baru); `src/lib/hooks/select/health-service/health-service-select-resources.js` (diubah); `src/lib/hooks/select/select-resource-registry.js` (diubah); `src/utils/menu-sidebar/menu-items.jsx` (diubah)

## Yang dibangun

Layar `/health-services/inpatient-management/admissions`, menu **Admisi Rawat Inap**. Satu halaman
dengan tiga bagian berurutan: isian admisi, kebutuhan isolasi, lalu pemilihan tempat tidur. Papan
tempat tidurnya adalah komponen yang sama persis dengan `FE-RWI-005`, kali ini dengan `episodeId`
terisi sehingga server menyaring memakai seluruh aturan kelayakan milik episode tersebut.

Registry select frontend belum punya sumber daya **pasien**, padahal layar ini membutuhkannya.
Sumber daya itu ditambahkan menunjuk `GET /health-services/patient-management/master-data/patients/options`
yang memang sudah ada, beserta aliasnya. Keempat isian select juga diberi `optionResource` eksplisit
supaya layar ini tidak bergantung pada kelengkapan alias registry — `patientClassId` misalnya
ternyata belum punya alias sama sekali.

- IMPLEMENTATION: (1) Isian admisi memakai `BaseEditorForm` dengan empat select dan satu catatan, sama seperti form master data lain. (2) Setelah episode `Draft` lahir, isian admisi dikunci dan bagian isolasi terbuka. (3) Menyalakan kebutuhan isolasi memunculkan isian keterangan; tombol simpan tetap mati selama keterangannya kosong. (4) Setelah kebutuhan isolasi tersimpan, papan tempat tidur pada layar yang sama dimuat ulang lewat `bedRefreshToken` — penyaringnya tetap milik server. (5) `IsolationSource` tidak pernah dikirim; server yang menentukannya, supaya catatan petugas admisi tidak dapat menyamar sebagai keputusan klinis DPJP.
- API CONTRACT IMPACT: Tidak mengubah kontrak.
- DATABASE IMPACT: Tidak ada.
- SECURITY IMPACT: Tidak mengubah authorization maupun authentication.
- VISUAL REFERENCE: NOT REQUIRED — roadmap membebaskan pemakaian tab, modal, atau drawer.
- WEWENANG UI YANG DIPAKAI: "Pemakaian tab, modal, atau drawer bebas, selama aturan pengiriman ganda dipenuhi". Dipilih **satu halaman bertahap**, karena kriteria 3 menuntut pencarian tempat tidur berada di layar yang sama dengan penetapan isolasi.

## Acceptance criteria

| Kriteria | Hasil | Bukti |
| --- | :---: | --- |
| 1. Admisi tanpa DPJP ditolak, dan pesannya menyebut DPJP wajib | **LULUS** | e2e menekan Buka Admisi pada form kosong; pesan "DPJP wajib dipilih. Episode rawat inap tidak boleh tersimpan tanpa dokter penanggung jawab." muncul, dan nol permintaan `POST /episodes` terkirim |
| 2. Menyalakan kebutuhan isolasi wajib disertai keterangan | **LULUS** | e2e: begitu isolasi dinyalakan, tombol **Simpan Kebutuhan Isolasi** `disabled`; setelah keterangan diisi tombolnya hidup |
| 3. Setelah kebutuhan isolasi disetel, pencarian tempat tidur pada layar yang sama ikut tersaring | **LULUS** | e2e: sebelum disetel, `BD-001` dapat dipilih dan `BD-009` tidak; sesudah isolasi tersimpan keduanya bertukar keadaan tanpa pindah halaman |
| 4. Menekan tombol simpan dua kali hanya menghasilkan satu episode | **LULUS** | e2e menunda balasan `POST` 1,5 detik lalu mengirim dua klik berturut-turut dalam satu evaluate; server tiruan mencatat `createdEpisodeCount === 1` |
| 5. Isian tidak hilang ketika server menolak | **LULUS** | Test kode membuktikan jalur `catch` `handleSubmitAdmission` tidak menyentuh `setForm`; e2e penempatan membuktikan catatan yang diketik tetap ada sesudah 409 dan 422 |

- VALIDATION: e2e `tests/e2e/inpatient-admission.spec.mjs` | PASS, 6/6 | TASK | satu jalur berhasil, tiga jalur gagal (tanpa DPJP, 422, 409), pengiriman ganda, dan arah pesan isolasi
- VALIDATION: `node --test tests/unit/inpatient-admission.test.mjs` | PASS, 8/8 | TASK | pesan DPJP, payload admisi, keharusan keterangan isolasi, larangan mengirim `IsolationSource`, kelengkapan isian, penjaga pengiriman ganda, jalur gagal tidak mengosongkan isian, dan pemicu pembacaan ulang tempat tidur
- VALIDATION: `npm run lint:errors` | PASS, exit 0 | TASK
- VALIDATION: `npm run build` beserta `postbuild` | PASS, exit 0 | TASK | route `/health-services/inpatient-management/admissions` terbaca pada keluaran build
- VALIDATION: seluruh unit test kecuali berkas yang rusak sejak merge | PASS, 82/82 | TASK
- MANUAL TEST: NOT FEASIBLE — memerlukan akun berhak `InpatientEpisode : Create` dan `SetIsolation`, serta data pasien dan dokter yang siap. Seluruh kontrol — keempat select, sakelar isolasi, isian keterangan, tombol simpan, pemilihan tempat tidur, dan tombol penempatan — dijalankan di browser sungguhan lewat e2e dengan API tiruan.
- WARNINGS: (1) Jalur kunjungan (`EncounterId`) sengaja tidak disediakan layar ini; admisi selalu memakai jalur pasien datang langsung, dan server yang membuatkan kunjungannya. Jalur menunjuk kunjungan yang sudah ada belum punya layarnya. (2) `MotherEpisodeId` untuk bayi rawat gabung juga belum disediakan — itu scope `FE-RWI` bayi baru lahir yang belum ada task-nya pada slice ini.
- KNOWN ISSUES: `patientClassId` belum punya alias pada `select-resource-registry.js`. Layar ini tidak terdampak karena memakai `optionResource` eksplisit, tetapi layar lain yang mengandalkan alias akan mendapati select kelas kosong.
- DEPENDENCY BACKEND: `BE-RWI-007` dan `BE-RWI-014` — kedua endpoint berstatus ✅ `Tersedia`. `BE-RWI-014` masih 🟡 di roadmap backend, tetapi penahannya kriteria 403 di sisi backend, bukan ketersediaan endpoint.
- INCIDENTAL CHANGES: `test-results/.last-run.json` dipulihkan; konfigurasi Playwright sementara dihapus.
- INTERRUPTIONS: NONE
- GIT STATUS: Berkas baru dan tiga berkas diubah, **belum di-stage dan belum di-commit**.
- NEXT RECOMMENDED STEP: Putuskan apakah jalur admisi dari kunjungan yang sudah ada perlu layar sendiri sebelum MVP dirilis.
