# Roadmap Frontend Bed Management

| Metadata | Nilai |
| --- | --- |
| blueprint_id / pemilik sub-modul | RWI-BP-001 / episode-rawat-inap (COMPOSITE; ownership module-map9) |
| planning_id / revision / tanggal | BM-DEL-20261011-01 / 1 / 2026-10-11 Asia/Jakarta |
| status / approval | Pengguna mengotorisasi seluruh7task frontend secara eksplisit pada sesi 2026-10-11; source tersedia, status 🟡 sebagian; bukan persetujuan aktivasi/runtime |
| blueprint current / contract target | Child manifest revision11 draft; target0.12.0 menjadi masukan yang diotorisasi pengguna. Lock existing APPROVED_BACKEND_IMPLEMENTATION_PARTIAL tetap; approval frontend sesi dicatat pada laporan ini, tanpa mengubah lock/manifest |
| input revisions | Decision46; gate1.12 BM-RCG-20261010-01; module-map6; impact BM-IMP-20261011-01 rev1 |
| source SHA backend | db85bede83c0ddbc9aaa412c576f2c3fd7447492 |
| source SHA frontend | 5aa2c7c70754e6131e4243b5ea3ee5f768b7a5d2 |
| hash lock | [bed-management-delivery-lock.json](bed-management-delivery-lock.json): 24/24 input raw-byte SHA256 cocok pada 2026-10-11; lock scope backend tidak ditulis oleh task frontend |
| scope / ID | 7 task FE-RWI-231–237; source frontend serta laporan/status/tautan bukti di blueprint backend; source backend tidak diubah |
| batas sesi | Source frontend dan bukti task dikerjakan; build/lint tidak dijalankan sesuai instruksi pengguna. Tidak ada migration/database/deploy/stage/commit/push |
| traceability | [requirement-traceability-bed-management.md](requirement-traceability-bed-management.md) |

Legenda status: ✅ selesai dengan laporan dan bukti; 🟡 sebagian dengan jumlah AC terpenuhi; ⛔ terblokir oleh prasyarat; tanpa tanda belum dikerjakan. Source ketujuh task sudah dikerjakan; belum ada task ✅ karena acceptance runtime belum terbukti.

## Grafik Urutan Dependency

```text
BE-RWI-211 🟡 [BE]────>FE-RWI-231 🟡──┐
                                      │
                                      │    BE-RWI-213 🟡 [BE]──┐
                                      │                        │
                                      ├────────────────────────┴───>FE-RWI-232 🟡
                                      │
                                      │    BE-RWI-215 🟡 [BE]──┐
                                      │                        │
                                      ├────────────────────────┴───>FE-RWI-233 🟡
                                      │
                                      │    BE-RWI-217 🟡 [BE]──┐
                                      │                        │
                                      ├────────────────────────┴───>FE-RWI-234 🟡
                                      │
                                      │    BE-RWI-219 🟡 [BE]──┐
                                      │                        │
                                      ├────────────────────────┴───>FE-RWI-235 🟡
                                      │
                                      │    BE-RWI-220 🟡 [BE]──┐
                                      │                        │
                                      ├────────────────────────┴───>FE-RWI-236 🟡
                                      │
                                      │    BE-RWI-221 🟡 [BE]──┐
                                      │                        │
                                      └────────────────────────┴───>FE-RWI-237 🟡
```

`[BE]` = cermin baca-saja task pada [backend-roadmap-bed-management.md](backend-roadmap-bed-management.md), status 🟡 mengikuti pemilik (source ada, runtime belum terbukti). Persetujuan source FE diberikan pengguna pada sesi ini; bukti dependency runtime belum tertutup. Cermin tidak dihitung sebagai task frontend atau gelombang frontend. Masukan tiap cabang adalah dependency gabungan, bukan alternatif.

| Gelombang | Boleh mulai setelah | Task |
| --- | --- | --- |
| 🟡 BM-A | `BE-RWI-211` | `FE-RWI-231` |
| 🟡 BM-A | `FE-RWI-231`, `BE-RWI-213` | `FE-RWI-232` |
| 🟡 BM-A | `FE-RWI-231`, `BE-RWI-215` | `FE-RWI-233` |
| 🟡 BM-A | `FE-RWI-231`, `BE-RWI-217` | `FE-RWI-234` |
| 🟡 BM-A | `FE-RWI-231`, `BE-RWI-219` | `FE-RWI-235` |
| 🟡 BM-A | `FE-RWI-231`, `BE-RWI-220` | `FE-RWI-236` |
| 🟡 BM-A | `FE-RWI-231`, `BE-RWI-221` | `FE-RWI-237` |

Label gelombang BM-A dari perencanaan dipertahankan; hanya tanda status diperbarui. Persetujuan eksplisit sesi membuka pekerjaan source tujuh task; dependency backend baru terbukti source 🟡. Penomoran/urutan dependency tidak diubah oleh implementasi frontend. BM-G01–04 adalah gate aktivasi spesifik pada traceability; tidak dipasang sebagai penghalang global desain atau seluruh development.

## Register Task

| Task ID / status | Outcome | Requirement / decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko / pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| `FE-RWI-231` 🟡 | Menu Bed Management dan Monitoring memakai hak, status serta respons fresh | FR-BM-001, FR-BM-002, FR-BM-004; RWI-DEC-274, RWI-DEC-287, RWI-DEC-289, RWI-DEC-290, RWI-DEC-292 | 0.12.0 draft; lock BM-DEL-20261011-01 | BM-CAP-02, BM-CAP-03, BM-CAP-07, BM-CAP-15 | Satu consumer bisnis; detail kartu | `BE-RWI-211` | RWI-AC-396, RWI-AC-397, RWI-AC-398, RWI-AC-416, RWI-AC-417, RWI-AC-420, RWI-AC-422, RWI-AC-424 | V-FE + BM-AT-396, BM-AT-397, BM-AT-398, BM-AT-416, BM-AT-417, BM-AT-420, BM-AT-422, BM-AT-424 | Owner FE dalam mandat DEC-292; privasi actual BM-G03. Utility bersama milik task ini dipakai task berikutnya. | D-FE parsial; unit58/58 PASS; runtime NOT_RUN; [laporan](../task/report/frontend/FE-RWI-231.md) |
| `FE-RWI-232` 🟡 | Reservasi, pembatalan, penempatan dan departure mengirim versi serta key yang benar | FR-BM-003, FR-BM-004; RWI-DEC-281, RWI-DEC-283, RWI-DEC-288, RWI-DEC-289, RWI-DEC-290 | 0.12.0 draft; lock BM-DEL-20261011-01 | BM-CAP-04, BM-CAP-05, BM-CAP-08, BM-CAP-14 | Satu consumer bisnis; detail kartu | `FE-RWI-231`, `BE-RWI-213` | RWI-AC-407, RWI-AC-408, RWI-AC-413, RWI-AC-418, RWI-AC-419, RWI-AC-421, RWI-AC-422, RWI-AC-426 | V-FE + BM-AT-407, BM-AT-408, BM-AT-413, BM-AT-418, BM-AT-419, BM-AT-421, BM-AT-422, BM-AT-426 | Owner FE; harus mendata semua callsite lama agar perubahan kontrak tidak memutus admisi/discharge. | D-FE parsial; unit58/58 PASS; runtime NOT_RUN; [laporan](../task/report/frontend/FE-RWI-232.md) |
| `FE-RWI-233` 🟡 | Child pembersihan mendukung HK dan verifikator sesuai hak masing-masing | FR-BM-005, FR-BM-006; RWI-DEC-278, RWI-DEC-280, RWI-DEC-282, RWI-DEC-287, RWI-DEC-293 | 0.12.0 draft; lock BM-DEL-20261011-01 | BM-CAP-07, BM-CAP-09, BM-CAP-15 | Satu consumer bisnis; detail kartu | `FE-RWI-231`, `BE-RWI-215` | RWI-AC-406, RWI-AC-409, RWI-AC-410, RWI-AC-411, RWI-AC-416, RWI-AC-417, RWI-AC-421, RWI-AC-425 | V-FE + BM-AT-406, BM-AT-409, BM-AT-410, BM-AT-411, BM-AT-416, BM-AT-417, BM-AT-421, BM-AT-425 | BM-G02/G03 untuk UAT nyata; owner FE dan pihak operasional yang berwenang. | D-FE parsial; unit58/58 PASS; runtime NOT_RUN; [laporan](../task/report/frontend/FE-RWI-233.md) |
| `FE-RWI-234` 🟡 | Form master bed dan hierarchy mengikuti reasons, affected versions dan outcome | FR-BM-007, FR-BM-008; RWI-DEC-285, RWI-DEC-288, RWI-DEC-289 | 0.12.0 draft; lock BM-DEL-20261011-01 | BM-CAP-01, BM-CAP-10, BM-CAP-14 | Satu consumer bisnis; detail kartu | `FE-RWI-231`, `BE-RWI-217` | RWI-AC-412, RWI-AC-419, RWI-AC-421, RWI-AC-424 | V-FE + BM-AT-412, BM-AT-419, BM-AT-421, BM-AT-424 | Owner FE bersama seluruh tim MasterData. Shared consumer files perlu koordinasi urutan diff agar tidak menimpa perubahan task lain. | D-FE parsial; unit58/58 PASS; runtime NOT_RUN; [laporan](../task/report/frontend/FE-RWI-234.md) |
| `FE-RWI-235` 🟡 | Transfer menampilkan asal otomatis dan meminta kategori manual yang tervalidasi | FR-BM-002, FR-BM-009, FR-BM-010; RWI-DEC-275, RWI-DEC-276, RWI-DEC-277, RWI-DEC-284, RWI-DEC-289, RWI-DEC-291, RWI-DEC-292 | 0.12.0 draft; lock BM-DEL-20261011-01 | BM-CAP-03, BM-CAP-11, BM-CAP-12, BM-CAP-15 | Satu consumer bisnis; detail kartu | `FE-RWI-231`, `BE-RWI-219` | RWI-AC-399, RWI-AC-400, RWI-AC-401, RWI-AC-403, RWI-AC-404, RWI-AC-405, RWI-AC-414, RWI-AC-420, RWI-AC-423 | V-FE + BM-AT-399, BM-AT-400, BM-AT-401, BM-AT-403, BM-AT-404, BM-AT-405, BM-AT-414, BM-AT-420, BM-AT-423 | BM-G01 untuk lintas kelas actual; owner FE mengikuti kontrak target yang disetujui. | D-FE parsial; unit58/58 PASS; runtime NOT_RUN; [laporan](../task/report/frontend/FE-RWI-235.md) |
| `FE-RWI-236` 🟡 | Bed Usage History menampilkan hunian awal, snapshot dan semua versi koreksi | FR-BM-011, FR-BM-012; RWI-DEC-286, RWI-DEC-287, RWI-DEC-288, RWI-DEC-292 | 0.12.0 draft; lock BM-DEL-20261011-01 | BM-CAP-12, BM-CAP-13, BM-CAP-14, BM-CAP-15, BM-CAP-17 | Satu consumer bisnis; detail kartu | `FE-RWI-231`, `BE-RWI-220` | RWI-AC-402, RWI-AC-415, RWI-AC-416, RWI-AC-417, RWI-AC-418 | V-FE + BM-AT-402, BM-AT-415, BM-AT-416, BM-AT-417, BM-AT-418 | Owner FE; Billing guard tetap server canonical. Pemilik privacy actual masih gateBM-G03. | D-FE parsial; unit58/58 PASS; runtime NOT_RUN; [laporan](../task/report/frontend/FE-RWI-236.md) |
| `FE-RWI-237` 🟡 | Helper admisi transfer IGD dan inventaris cutover mengikuti mutation target | FR-BM-003, FR-BM-004; RWI-DEC-283, RWI-DEC-289, RWI-DEC-290, RWI-DEC-293 | 0.12.0 draft; lock BM-DEL-20261011-01 | BM-CAP-04, BM-CAP-05, BM-CAP-14, BM-CAP-16 | Satu consumer bisnis; detail kartu | `FE-RWI-231`, `BE-RWI-221` | RWI-AC-408, RWI-AC-419, RWI-AC-421, RWI-AC-422, RWI-AC-425 | V-FE + BM-AT-408, BM-AT-419, BM-AT-421, BM-AT-422, BM-AT-425 | Owner FE/InPatientManagement; environment owner melaksanakan cutover hanya dengan otorisasi terpisah. | D-FE parsial; unit58/58 PASS; runtime NOT_RUN; [laporan](../task/report/frontend/FE-RWI-237.md) |

## Batas Handoff dan Definition of Done

### D-FE — bukti wajib setiap task frontend

Periksa AGENTS.md frontend dan aturan frontend canonical saat eksekusi, branch/authority, kontrak approved beserta hash dan dependency backend. Pertahankan Next.js App Router, JavaScript/JSX, Redux dan Axios existing; struktur route/view→hook→service existing. Setiap elemen UI mempunyai keputusan reuse/extension/new beserta alasan dari base-component catalog/decision gate, token dan accessibility. Detail nonkritis tetap DEV_DISCRETION menurut RWI-DEC-292; planning ini tidak memberikan approval visual baru.

V-FE mencakup diff/scope review, lint:errors, test:unit dan build sesuai scripts serta baseline AGENTS frontend saat eksekusi; unit pure logic baru hanya bila bernilai menurut rules/frontend/test-policy.md, memakai node:test existing. Jangan menambah framework. Rekam request/network, screen state pending/empty/error/success/403/409/422 serta hasil skenario pada kartu. Live browser/E2E hanya setelah lingkungan, fixture dan wewenang tersedia; belum ada bukti browser/UAT di planning ini. Tidak menjalankan test:uat atau package manager yang mengubah lockfile pada fase ini.

D-FE terpenuhi bila source dan seluruh AC kartu terbukti, status server menjadi sumber tampilan, UI tidak membocorkan data, laporan tracked kelak tersedia di task/report/frontend/<TASK-ID>.md, dan register/grafik/gelombang/traceability diperbarui konsisten. Ketujuh laporan task sudah tersedia, ditautkan pada kartu/register. Build/lint dikecualikan pengguna; unit scope58/58 PASS dan suite penuh2694/2720 dengan26 kegagalan baseline. Browser/API/database NOT_RUN. FE tidak membuat migration, role grant, tabel atau backend alternatif.

## Kartu Task

### 🟡 FE-RWI-231 — Menu Bed Management dan Monitoring memakai hak, status serta respons fresh

| Field | Isi |
| --- | --- |
| Status | 🟡 SEBAGIAN — 2026-10-11; source 3/4 lengkap + 1/4 parsial (gap respons backend); acceptance penuh0/4, browser/API NOT_RUN. Unit scope58/58 PASS; suite2720:2694 PASS/26 FAIL baseline. Build/lint dikecualikan pengguna. Dependency backend/runtime dan gate aktual masih terbuka; [laporan](../task/report/frontend/FE-RWI-231.md). |
| Slice / outcome | Menu Bed Management dan Monitoring memakai hak, status serta respons fresh |
| Ukuran / batas | Satu alur consumer bisnis; gunakan komponen dan helper existing. |
| Jejak | FR-BM-001, FR-BM-002, FR-BM-004; RWI-DEC-274, RWI-DEC-287, RWI-DEC-289, RWI-DEC-290, RWI-DEC-292; RWI-AC-396, RWI-AC-397, RWI-AC-398, RWI-AC-416, RWI-AC-417, RWI-AC-420, RWI-AC-422, RWI-AC-424; NFR-702, NFR-704 |
| Desain / kontrak | FE15.1–15.7; API13.1–13.4; Flow14; target0.12.0; hash draft di delivery-lock; approval upstream tidak diwariskan dari roadmap admisi IGD lama. |
| Reuse / disposisi | EXTEND Hero/SummaryGrid/StatusBadge/DataFilter/ResourceFilterSelect/Card/BaseButton/InfoToast/AccessDenied existing; utility request tetap mengikuti Axios hooks existing. |
| Cakupan | Rename satu leaf di route existing; tiga tab sesuai metadata. Monitoring enam status/counts dan child action shells berdasarkan AvailableActions. Service/hook shared untuk stable operation key, pending/reload/outcome handling; jangan mount tab yang dilarang atau invent grant. Refetch tunggu hasil promise aktual dan cegah respons lama menimpa hasil baru. |
| Batas source saat implementasi | src/utils/menu-sidebar/menu-items.jsx; existing bed-board view/hooks/utils; service Bed Management baru dalam struktur src/lib/services. |
| Dependency langsung | `BE-RWI-211` |
| Pekerjaan independen yang boleh sekarang | Source telah dikerjakan atas instruksi eksplisit pengguna; laporan mencatat bukti unit/statis dan sisa runtime. Gate runtime tidak diselesaikan dengan data buatan. |
| Risiko / owner / aktivasi | Owner FE dalam mandat DEC-292; privasi actual BM-G03. Utility bersama milik task ini dipakai task berikutnya. |
| Verifikasi yang wajib | V-FE + BM-AT-396, BM-AT-397, BM-AT-398, BM-AT-416, BM-AT-417, BM-AT-420, BM-AT-422, BM-AT-424 (semua NOT_RUN). Skenario matriks adalah rujukan bisnis; jangan menganggap test lama sebagai bukti perilaku target. |
| Definition of Done | D-FE dan seluruh AC positif/negatif di bawah terbukti; laporan task tracked tersedia. Source yang ada tanpa bukti penuh tidak boleh ✅. |
| Laporan | [Laporan FE-RWI-231](../task/report/frontend/FE-RWI-231.md) — tersedia; belum stage/commit. |

Acceptance positif:

- Satu leaf Bed Management, Monitoring/Transfer/Bed Usage History reachable sesuai hak; HK hanya lokasi/status.
- Confirm disabled selama loading/refresh/unknown; timeout diperiksa lewat own outcome, tidak mengklaim success atau mengganti key otomatis.

Acceptance negatif:

- Raw enum/cache tidak menentukan bookable; tab forbidden tidak fetch PHI.
- Tidak optimistic/offline overwrite; layout nonkritis DEV_DISCRETION dalam token/base components.

### 🟡 FE-RWI-232 — Reservasi, pembatalan, penempatan dan departure mengirim versi serta key yang benar

| Field | Isi |
| --- | --- |
| Status | 🟡 SEBAGIAN — 2026-10-11; source 4/4 dipetakan; acceptance penuh0/4, browser/API NOT_RUN. Unit scope58/58 PASS; suite2720:2694 PASS/26 FAIL baseline. Build/lint dikecualikan pengguna. Dependency backend/runtime dan gate aktual masih terbuka; [laporan](../task/report/frontend/FE-RWI-232.md). |
| Slice / outcome | Reservasi, pembatalan, penempatan dan departure mengirim versi serta key yang benar |
| Ukuran / batas | Satu alur consumer bisnis; gunakan komponen dan helper existing. |
| Jejak | FR-BM-003, FR-BM-004; RWI-DEC-281, RWI-DEC-283, RWI-DEC-288, RWI-DEC-289, RWI-DEC-290; RWI-AC-407, RWI-AC-408, RWI-AC-413, RWI-AC-418, RWI-AC-419, RWI-AC-421, RWI-AC-422, RWI-AC-426; NFR-704, NFR-705, NFR-706 |
| Desain / kontrak | FE15.4–15.7; API13.2–13.4; Flow09/14; target0.12.0; hash draft di delivery-lock; approval upstream tidak diwariskan dari roadmap admisi IGD lama. |
| Reuse / disposisi | EXTEND hooks/API existing; reuse ConfirmModal/SidePanel/input/InfoToast; tanpa form admisi baru. |
| Cakupan | Perbarui bed actions dan seluruh callsite reserve/cancel/place/departure yang ditemukan audit: admission-bed/admission-flow/episode-detail. Kirim ExpectedBedVersion/current placement/key dari fresh server response; cancel reason wajib. Countdown120m hanya tampilan; gunakan shared pending/outcome utility. |
| Batas source saat implementasi | use-inpatient-bed-board-actions.jsx; use-inpatient-admission-bed.jsx; use-inpatient-admission-flow.jsx; use-inpatient-episode-detail.jsx; bed/discharge services existing. |
| Dependency langsung | `FE-RWI-231`, `BE-RWI-213` |
| Pekerjaan independen yang boleh sekarang | Source telah dikerjakan atas instruksi eksplisit pengguna; laporan mencatat bukti unit/statis dan sisa runtime. Gate runtime tidak diselesaikan dengan data buatan. |
| Risiko / owner / aktivasi | Owner FE; harus mendata semua callsite lama agar perubahan kontrak tidak memutus admisi/discharge. |
| Verifikasi yang wajib | V-FE + BM-AT-407, BM-AT-408, BM-AT-413, BM-AT-418, BM-AT-419, BM-AT-421, BM-AT-422, BM-AT-426 (semua NOT_RUN). Skenario matriks adalah rujukan bisnis; jangan menganggap test lama sebagai bukti perilaku target. |
| Definition of Done | D-FE dan seluruh AC positif/negatif di bawah terbukti; laporan task tracked tersedia. Source yang ada tanpa bukti penuh tidak boleh ✅. |
| Laporan | [Laporan FE-RWI-232](../task/report/frontend/FE-RWI-232.md) — tersedia; belum stage/commit. |

Acceptance positif:

- Bed WaitingCleaning tidak bisa dipilih; cancel reason/versi/header terkirim; unused expiry mengikuti hasil server.
- Departure mengirim ExpectedPlacementId dan menangani old closure tanpa mengosongkan UI pasien B.

Acceptance negatif:

- Stale409 reload dan minta konfirmasi ulang; unknown hasil tidak diulang dengan key baru.
- Tidak menambah reminder/perpanjangan atau memutuskan expiry di browser.

### 🟡 FE-RWI-233 — Child pembersihan mendukung HK dan verifikator sesuai hak masing-masing

| Field | Isi |
| --- | --- |
| Status | 🟡 SEBAGIAN — 2026-10-11; source 3/4 lengkap + 1/4 parsial (gap respons backend); acceptance penuh0/4, browser/API NOT_RUN. Unit scope58/58 PASS; suite2720:2694 PASS/26 FAIL baseline. Build/lint dikecualikan pengguna. Dependency backend/runtime dan gate aktual masih terbuka; [laporan](../task/report/frontend/FE-RWI-233.md). |
| Slice / outcome | Child pembersihan mendukung HK dan verifikator sesuai hak masing-masing |
| Ukuran / batas | Satu alur consumer bisnis; gunakan komponen dan helper existing. |
| Jejak | FR-BM-005, FR-BM-006; RWI-DEC-278, RWI-DEC-280, RWI-DEC-282, RWI-DEC-287, RWI-DEC-293; RWI-AC-406, RWI-AC-409, RWI-AC-410, RWI-AC-411, RWI-AC-416, RWI-AC-417, RWI-AC-421, RWI-AC-425; NFR-702, NFR-703, NFR-704 |
| Desain / kontrak | FE15.5; API13.2–13.4; Flow11; target0.12.0; hash draft di delivery-lock; approval upstream tidak diwariskan dari roadmap admisi IGD lama. |
| Reuse / disposisi | REUSE Card/SidePanel/DataTable/StatusBadge/BaseButton/ConfirmModal/field components; wrapper fitur boleh baru sesuai base-component decision gate. |
| Cakupan | Child view Monitoring untuk mulai/selesai/riwayat attempt dan verifikasi/reject. Gunakan cycle/attempt/version server; EvidenceReference wajib, reject reason wajib. AvailableActions memisahkan fungsi HK dan perawat; label Menunggu verifikasi berada di Dalam Pembersihan. |
| Batas source saat implementasi | Bed Management child panel/hook/service; utility dari FE-RWI-231. |
| Dependency langsung | `FE-RWI-231`, `BE-RWI-215` |
| Pekerjaan independen yang boleh sekarang | Source telah dikerjakan atas instruksi eksplisit pengguna; laporan mencatat bukti unit/statis dan sisa runtime. Gate runtime tidak diselesaikan dengan data buatan. |
| Risiko / owner / aktivasi | BM-G02/G03 untuk UAT nyata; owner FE dan pihak operasional yang berwenang. |
| Verifikasi yang wajib | V-FE + BM-AT-406, BM-AT-409, BM-AT-410, BM-AT-411, BM-AT-416, BM-AT-417, BM-AT-421, BM-AT-425 (semua NOT_RUN). Skenario matriks adalah rujukan bisnis; jangan menganggap test lama sebagai bukti perilaku target. |
| Definition of Done | D-FE dan seluruh AC positif/negatif di bawah terbukti; laporan task tracked tersedia. Source yang ada tanpa bukti penuh tidak boleh ✅. |
| Laporan | [Laporan FE-RWI-233](../task/report/frontend/FE-RWI-233.md) — tersedia; belum stage/commit. |

Acceptance positif:

- HK selesai lalu status belum Available; verifier sah melihat evidence/reason dan dapat menerima/menolak.
- Attempt terdahulu tetap terlihat, tanpa patient/episode IDs dan tanpa checklist buatan.

Acceptance negatif:

- Deep-link atau button tersembunyi tidak menggantikan403 server; stale cycle meminta reload.
- Tidak menambah tab/status ketujuh atau timer pengesahan.

### 🟡 FE-RWI-234 — Form master bed dan hierarchy mengikuti reasons, affected versions dan outcome

| Field | Isi |
| --- | --- |
| Status | 🟡 SEBAGIAN — 2026-10-11; source 4/4 dipetakan; acceptance penuh0/4, browser/API NOT_RUN. Unit scope58/58 PASS; suite2720:2694 PASS/26 FAIL baseline. Build/lint dikecualikan pengguna. Dependency backend/runtime dan gate aktual masih terbuka; [laporan](../task/report/frontend/FE-RWI-234.md). |
| Slice / outcome | Form master bed dan hierarchy mengikuti reasons, affected versions dan outcome |
| Ukuran / batas | Satu alur consumer bisnis; gunakan komponen dan helper existing. |
| Jejak | FR-BM-007, FR-BM-008; RWI-DEC-285, RWI-DEC-288, RWI-DEC-289; RWI-AC-412, RWI-AC-419, RWI-AC-421, RWI-AC-424; NFR-704, NFR-706 |
| Desain / kontrak | FE15.6–15.7; API13.2/13.6–13.7; Flow12; target0.12.0; hash draft di delivery-lock; approval upstream tidak diwariskan dari roadmap admisi IGD lama. |
| Reuse / disposisi | EXTEND form/master service existing; reuse reason field/ConfirmModal/BaseButton/InfoToast; tidak mengganti CRUD wholesale. |
| Cakupan | Perbarui consumer master Bed/Room/ServiceUnit/PatientClass PUT/status/availability/delete/create yang terdampak. GET detail versi/affected-set sebelum confirm; OperationReason/Idempotency-Key wajib sesuai contract. Jangan mengarang versi atau menawarkan status sistem sebagai pilihan bebas. |
| Batas source saat implementasi | Existing master bed/room/service-unit/patient-class service/hooks/dialog consumers; inventaris path final wajib dilengkapi pada preflight. |
| Dependency langsung | `FE-RWI-231`, `BE-RWI-217` |
| Pekerjaan independen yang boleh sekarang | Source telah dikerjakan atas instruksi eksplisit pengguna; laporan mencatat bukti unit/statis dan sisa runtime. Gate runtime tidak diselesaikan dengan data buatan. |
| Risiko / owner / aktivasi | Owner FE bersama seluruh tim MasterData. Shared consumer files perlu koordinasi urutan diff agar tidak menimpa perubahan task lain. |
| Verifikasi yang wajib | V-FE + BM-AT-412, BM-AT-419, BM-AT-421, BM-AT-424 (semua NOT_RUN). Skenario matriks adalah rujukan bisnis; jangan menganggap test lama sebagai bukti perilaku target. |
| Definition of Done | D-FE dan seluruh AC positif/negatif di bawah terbukti; laporan task tracked tersedia. Source yang ada tanpa bukti penuh tidak boleh ✅. |
| Laporan | [Laporan FE-RWI-234](../task/report/frontend/FE-RWI-234.md) — tersedia; belum stage/commit. |

Acceptance positif:

- Held bed/child menampilkan409 dan tetap utuh; close kosong beralasan berhasil, reopen belum siap ditampilkan jujur.
- Read versions tidak membawa PHI; payload DELETE body target diselaraskan Axios client.

Acceptance negatif:

- Generic edit/status tidak menjadi bypass; changed affected-set meminta reload bukan merge buta.
- Tidak membuat master baru atau mengambil alih desain master di luar adapter kontrak.

### 🟡 FE-RWI-235 — Transfer menampilkan asal otomatis dan meminta kategori manual yang tervalidasi

| Field | Isi |
| --- | --- |
| Status | 🟡 SEBAGIAN — 2026-10-11; source 4/4 dipetakan; acceptance penuh0/4, browser/API NOT_RUN. Unit scope58/58 PASS; suite2720:2694 PASS/26 FAIL baseline. Build/lint dikecualikan pengguna. Dependency backend/runtime dan gate aktual masih terbuka; [laporan](../task/report/frontend/FE-RWI-235.md). |
| Slice / outcome | Transfer menampilkan asal otomatis dan meminta kategori manual yang tervalidasi |
| Ukuran / batas | Satu alur consumer bisnis; gunakan komponen dan helper existing. |
| Jejak | FR-BM-002, FR-BM-009, FR-BM-010; RWI-DEC-275, RWI-DEC-276, RWI-DEC-277, RWI-DEC-284, RWI-DEC-289, RWI-DEC-291, RWI-DEC-292; RWI-AC-399, RWI-AC-400, RWI-AC-401, RWI-AC-403, RWI-AC-404, RWI-AC-405, RWI-AC-414, RWI-AC-420, RWI-AC-423; NFR-701, NFR-704 |
| Desain / kontrak | FE15.4–15.7; API13.2–13.4; Flow10/14; target0.12.0; hash draft di delivery-lock; approval upstream tidak diwariskan dari roadmap admisi IGD lama. |
| Reuse / disposisi | EXTEND existing transfer flow; reuse ResourceFilterSelect/SidePanel/ConfirmModal/fields/StatusBadge; cosmetics DEV_DISCRETION. |
| Cakupan | Tab Transfer dan consumer transfer existing episode detail memakai transfer-context, available-beds dan shared mutation utility. Asal read-only current; tujuan eligible; radio/select Down/Up/Same tanpa default. Tunggu refetch actual sebelum confirm, kirim kedua bed versions dan source placement/version serta key. Tampilkan Unknown/order unverified dengan pesan yang dapat ditindaklanjuti. |
| Batas source saat implementasi | Transfer tab/wrapper dan transfer modal/hook existing episode detail; bed-occupancy.service.js. |
| Dependency langsung | `FE-RWI-231`, `BE-RWI-219` |
| Pekerjaan independen yang boleh sekarang | Source telah dikerjakan atas instruksi eksplisit pengguna; laporan mencatat bukti unit/statis dan sisa runtime. Gate runtime tidak diselesaikan dengan data buatan. |
| Risiko / owner / aktivasi | BM-G01 untuk lintas kelas actual; owner FE mengikuti kontrak target yang disetujui. |
| Verifikasi yang wajib | V-FE + BM-AT-399, BM-AT-400, BM-AT-401, BM-AT-403, BM-AT-404, BM-AT-405, BM-AT-414, BM-AT-420, BM-AT-423 (semua NOT_RUN). Skenario matriks adalah rujukan bisnis; jangan menganggap test lama sebagai bukti perilaku target. |
| Definition of Done | D-FE dan seluruh AC positif/negatif di bawah terbukti; laporan task tracked tersedia. Source yang ada tanpa bukti penuh tidak boleh ✅. |
| Laporan | [Laporan FE-RWI-235](../task/report/frontend/FE-RWI-235.md) — tersedia; belum stage/commit. |

Acceptance positif:

- Same class beda bed + pilihan Same berhasil; source Waiting/destination Occupied setelah server success dan refresh.
- Wrong category422 atau stale409 tidak mengubah lokasi lokal; official order proof tidak ditebak.

Acceptance negatif:

- No default kategori, no copy origin dari cache closure, no target stale confirm.
- Tidak menambah penerimaan tujuan sebagai tahap transfer wajib atau membalik sukses akibat callback.

### 🟡 FE-RWI-236 — Bed Usage History menampilkan hunian awal, snapshot dan semua versi koreksi

| Field | Isi |
| --- | --- |
| Status | 🟡 SEBAGIAN — 2026-10-11; source 4/4 dipetakan; acceptance penuh0/4, browser/API NOT_RUN. Unit scope58/58 PASS; suite2720:2694 PASS/26 FAIL baseline. Build/lint dikecualikan pengguna. Dependency backend/runtime dan gate aktual masih terbuka; [laporan](../task/report/frontend/FE-RWI-236.md). |
| Slice / outcome | Bed Usage History menampilkan hunian awal, snapshot dan semua versi koreksi |
| Ukuran / batas | Satu alur consumer bisnis; gunakan komponen dan helper existing. |
| Jejak | FR-BM-011, FR-BM-012; RWI-DEC-286, RWI-DEC-287, RWI-DEC-288, RWI-DEC-292; RWI-AC-402, RWI-AC-415, RWI-AC-416, RWI-AC-417, RWI-AC-418; NFR-702, NFR-703, NFR-705 |
| Desain / kontrak | FE15.6; API13.2–13.4; Flow13; target0.12.0; hash draft di delivery-lock; approval upstream tidak diwariskan dari roadmap admisi IGD lama. |
| Reuse / disposisi | REUSE DataTable/Pagination/DatePicker/ResourceFilterSelect/SidePanel/StatusBadge dan correction form existing. |
| Cakupan | Tab History fokus bed/periode UTC dan filter kamar, pagination server. Tampilkan ongoing tanpa end fiktif, legacy ContextSource dan category nullable jujur. Detail versi/correction memakai hak existing Correct/Billing, seluruh affected bed versions dan key; reports transfer tetap fitur existing tersendiri. |
| Batas source saat implementasi | Usage-history tab/hook/service; adapter existing placement history/correction clients. |
| Dependency langsung | `FE-RWI-231`, `BE-RWI-220` |
| Pekerjaan independen yang boleh sekarang | Source telah dikerjakan atas instruksi eksplisit pengguna; laporan mencatat bukti unit/statis dan sisa runtime. Gate runtime tidak diselesaikan dengan data buatan. |
| Risiko / owner / aktivasi | Owner FE; Billing guard tetap server canonical. Pemilik privacy actual masih gateBM-G03. |
| Verifikasi yang wajib | V-FE + BM-AT-402, BM-AT-415, BM-AT-416, BM-AT-417, BM-AT-418 (semua NOT_RUN). Skenario matriks adalah rujukan bisnis; jangan menganggap test lama sebagai bukti perilaku target. |
| Definition of Done | D-FE dan seluruh AC positif/negatif di bawah terbukti; laporan task tracked tersedia. Source yang ada tanpa bukti penuh tidak boleh ✅. |
| Laporan | [Laporan FE-RWI-236](../task/report/frontend/FE-RWI-236.md) — tersedia; belum stage/commit. |

Acceptance positif:

- Pasien tanpa transfer muncul; snapshot/versi lama tetap terbaca sesudah rename/koreksi; IsEffectiveForBilling dari server.
- Viewer tanpa hak episode tidak memperoleh identitas/link pasien; HK tab forbidden tidak fetch.

Acceptance negatif:

- Billing CLOSED correction422 tidak diakali delete; IsSuperseded tidak dipakai membuang normal transfer.
- Tidak membawa batas31hari report ke history atau menyalin state pasien lokal.

### 🟡 FE-RWI-237 — Helper admisi transfer IGD dan inventaris cutover mengikuti mutation target

| Field | Isi |
| --- | --- |
| Status | 🟡 SEBAGIAN — 2026-10-11; source 4/4 dipetakan; acceptance penuh0/4, browser/API NOT_RUN. Unit scope58/58 PASS; suite2720:2694 PASS/26 FAIL baseline. Build/lint dikecualikan pengguna. Dependency backend/runtime dan gate aktual masih terbuka; [laporan](../task/report/frontend/FE-RWI-237.md). |
| Slice / outcome | Helper admisi transfer IGD dan inventaris cutover mengikuti mutation target |
| Ukuran / batas | Satu alur consumer bisnis; gunakan komponen dan helper existing. |
| Jejak | FR-BM-003, FR-BM-004; RWI-DEC-283, RWI-DEC-289, RWI-DEC-290, RWI-DEC-293; RWI-AC-408, RWI-AC-419, RWI-AC-421, RWI-AC-422, RWI-AC-425; NFR-704, NFR-706 |
| Desain / kontrak | FE15.7; BE14.14; API13.8; Acceptance22.6; target0.12.0; hash draft di delivery-lock; approval upstream tidak diwariskan dari roadmap admisi IGD lama. |
| Reuse / disposisi | EXTEND helper/service existing dan shared operation utility; list admission-transfer tidak diubah menjadi flow baru. |
| Cakupan | Perbarui admitFromTransfer helper/service untuk ExpectedBedVersion dan header key, HTTP201/OperationMeta/replay. Telusuri pemanggil nyata dan daftarkan semua mutation consumer pada handoff cutover bersama FE-RWI-232/234/235/236; helper yang belum dipanggil tidak dijadikan bukti UI. Tidak membangun wizard IGD baru. |
| Batas source saat implementasi | inpatient-admission-transfer.service.js dan caller existing yang terbukti; catatan cutover dalam laporan tracked task. |
| Dependency langsung | `FE-RWI-231`, `BE-RWI-221` |
| Pekerjaan independen yang boleh sekarang | Source telah dikerjakan atas instruksi eksplisit pengguna; laporan mencatat bukti unit/statis dan sisa runtime. Gate runtime tidak diselesaikan dengan data buatan. |
| Risiko / owner / aktivasi | Owner FE/InPatientManagement; environment owner melaksanakan cutover hanya dengan otorisasi terpisah. |
| Verifikasi yang wajib | V-FE + BM-AT-408, BM-AT-419, BM-AT-421, BM-AT-422, BM-AT-425 (semua NOT_RUN). Skenario matriks adalah rujukan bisnis; jangan menganggap test lama sebagai bukti perilaku target. |
| Definition of Done | D-FE dan seluruh AC positif/negatif di bawah terbukti; laporan task tracked tersedia. Source yang ada tanpa bukti penuh tidak boleh ✅. |
| Laporan | [Laporan FE-RWI-237](../task/report/frontend/FE-RWI-237.md) — tersedia; belum stage/commit. |

Acceptance positif:

- Helper mengirim version/key dari hasil available-beds, retry same key menerima episode yang sama.
- Daftar consumers menunjukkan route/caller/status target dan pemilik; setiap writer/consumer legacy yang belum aman menahan cutover BM-G04.

Acceptance negatif:

- Missing caller atau UAT yang belum tersedia dicatat NOT_RUN/GAP, tidak disebut alur live lulus.
- Tidak menjalankan migration/deploy/seed akses dari task frontend.

## Traceability dan Handoff Berikutnya

Setiap 7 task di register mempunyai requirement, decision, versi/hash kontrak, AC dan bukti yang diharapkan pada [traceability](requirement-traceability-bed-management.md). Source frontend dan laporan tujuh task tersedia atas otorisasi pengguna; gap saat ini adalah bukti runtime, dependency backend yang masih 🟡, metadata transfer, evidence historis dan proof gate aktual. Tidak adanya automated backend test bukan gap.

Handoff berikutnya: pengguna menjalankan build/lint manual; owner backend meninjau metadata tanpa key transfer dan respons evidence historis; lingkungan/role/fixture dipersiapkan untuk BM-AT scoped. File shared telah diadaptasi berurutan; source yang tersedia belum mengizinkan cutover/deploy atau menutup UAT.

## Status Implementasi Frontend — 2026-10-11

Tujuh task diotorisasi pengguna dan source dikerjakan berurutan; dependency source backend diterima sebagai masukan parsial, bukan bukti siap produksi. Hash24/24 cocok, parser/import63file bersih, UI anti-regresi9 JSX/CSS baru bersih dan diff-check PASS. Unit scope58/58 PASS; suite penuh2720test/2694 PASS/26 FAIL yang juga muncul pada pembanding virtual source HEAD. Build/lint NOT_RUN atas instruksi pengguna; browser NOT FEASIBLE karena localhost:3000 ECONNREFUSED. Gate BM-G01–04, metadata transfer dan evidence historis tetap terbuka.

- FE-RWI-231 🟡 — SEBAGIAN — 2026-10-11; source 3/4 lengkap + 1/4 parsial (gap respons backend); acceptance penuh0/4, browser/API NOT_RUN. Unit scope58/58 PASS; suite2720:2694 PASS/26 FAIL baseline. Build/lint dikecualikan pengguna. Dependency backend/runtime dan gate aktual masih terbuka; [laporan](../task/report/frontend/FE-RWI-231.md).
- FE-RWI-232 🟡 — SEBAGIAN — 2026-10-11; source 4/4 dipetakan; acceptance penuh0/4, browser/API NOT_RUN. Unit scope58/58 PASS; suite2720:2694 PASS/26 FAIL baseline. Build/lint dikecualikan pengguna. Dependency backend/runtime dan gate aktual masih terbuka; [laporan](../task/report/frontend/FE-RWI-232.md).
- FE-RWI-233 🟡 — SEBAGIAN — 2026-10-11; source 3/4 lengkap + 1/4 parsial (gap respons backend); acceptance penuh0/4, browser/API NOT_RUN. Unit scope58/58 PASS; suite2720:2694 PASS/26 FAIL baseline. Build/lint dikecualikan pengguna. Dependency backend/runtime dan gate aktual masih terbuka; [laporan](../task/report/frontend/FE-RWI-233.md).
- FE-RWI-234 🟡 — SEBAGIAN — 2026-10-11; source 4/4 dipetakan; acceptance penuh0/4, browser/API NOT_RUN. Unit scope58/58 PASS; suite2720:2694 PASS/26 FAIL baseline. Build/lint dikecualikan pengguna. Dependency backend/runtime dan gate aktual masih terbuka; [laporan](../task/report/frontend/FE-RWI-234.md).
- FE-RWI-235 🟡 — SEBAGIAN — 2026-10-11; source 4/4 dipetakan; acceptance penuh0/4, browser/API NOT_RUN. Unit scope58/58 PASS; suite2720:2694 PASS/26 FAIL baseline. Build/lint dikecualikan pengguna. Dependency backend/runtime dan gate aktual masih terbuka; [laporan](../task/report/frontend/FE-RWI-235.md).
- FE-RWI-236 🟡 — SEBAGIAN — 2026-10-11; source 4/4 dipetakan; acceptance penuh0/4, browser/API NOT_RUN. Unit scope58/58 PASS; suite2720:2694 PASS/26 FAIL baseline. Build/lint dikecualikan pengguna. Dependency backend/runtime dan gate aktual masih terbuka; [laporan](../task/report/frontend/FE-RWI-236.md).
- FE-RWI-237 🟡 — SEBAGIAN — 2026-10-11; source 4/4 dipetakan; acceptance penuh0/4, browser/API NOT_RUN. Unit scope58/58 PASS; suite2720:2694 PASS/26 FAIL baseline. Build/lint dikecualikan pengguna. Dependency backend/runtime dan gate aktual masih terbuka; [laporan](../task/report/frontend/FE-RWI-237.md).
