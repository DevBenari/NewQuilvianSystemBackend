# Bukti Acceptance Frontend Operasi — `FE-OPR-001` … `FE-OPR-009`

Audit per task terhadap bukti yang **sudah ada**, bukan terhadap status roadmap lama.

| Hal | Isi |
|---|---|
| Diaudit | 14 Oktober 2026 |
| Cabang frontend | `Ikbalv2` |
| Acuan acceptance | `roadmap/frontend-roadmap.md` kolom terakhir |
| Aturan | status tidak dinaikkan hanya karena berkas source ada |

## Mengapa dokumen ini dibuat

`testing/acceptance-test-matrix.md` memuat **nol** baris `FE-OPR`, dan
`testing/readiness-report.md` masih menyatakan *"0 dari 9 task"* serta *"belum ada satu pun layar
frontend"* — diaudit pada commit frontend `400104f2a0f3239c14c40f5905b419977a538450`, jauh
sebelum layarnya ada. Akibatnya pekerjaan yang sudah terkirim terbaca seolah belum dimulai.

Dokumen ini memisahkan tiga hal yang sering dicampur: **source ada**, **uji ada**, dan
**acceptance terbukti**.

## Temuan penting tentang asal uji

Sebagian layar Operasi dikerjakan tim **Rawat Inap** di bawah ID `FE-RWI-*`, bukan `FE-OPR-*`.
Ujinya karena itu membawa acceptance milik mereka. Dua di antaranya **tidak** menyentuh berkas
Operasi sama sekali, sehingga **tidak** dipakai sebagai bukti di sini:

| Uji | Menyentuh berkas Operasi? | Dipakai sebagai bukti |
|---|---|---|
| `operating-room-reject-case` | ✅ `case-detail-view`, `case-constant`, `case.service`, `reject-modal` | ya — `FE-OPR-001` |
| `ward-pre-op` | ✅ `ward-pre-op.service`, `ward-pre-op-drawer` | ya — `FE-OPR-003` (sebagian) |
| `post-op-summary` | ✅ `post-op-summary.service`, `drawer`, `utils` | ya — `FE-OPR-005` (sebagian) |
| `surgery-booking` | ❌ nol berkas Operasi | **tidak** |
| `transfer-handover` | ❌ nol berkas Operasi | **tidak** |

## Matriks

| Task | Requirement | Acceptance asli | Source | Uji | Runtime | Acceptance | Status |
|---|---|---|---|---|---|---|---|
| `FE-OPR-001` | `OPS-REQ-001/002` | loading/empty/error/retry/stale, permission, paging, tanpa data klinis di storage/log | `/cases`, `/cases/[slug]`, `/cases/create`; `case-list/detail/form/table-columns`; `operating-room-case.service.js` | `operating-room-reject-case` (4 AC), `operating-room-hardening` (10) | — | **sebagian**: permission + storage/log terbukti; **loading/empty/error/stale/paging nol uji** | `IN PROGRESS` |
| `FE-OPR-002` | `OPS-REQ-003/004` | konflik `409` terbaca, histori jadwal terlihat, duplicate submit dicegah | `/cases/[slug]/schedule`; `operating-room-schedule-view.jsx`; service | **nol uji menyentuh layar jadwal** | — | **belum** | `IN PROGRESS` |
| `FE-OPR-003` | `OPS-REQ-005` | fase/sign-off/bypass sesuai `availableActions`; error prasyarat jelas | `/preparation`; `operating-room-preparation-view.jsx`; `ward-pre-op/` | `ward-pre-op` (3 AC) — hanya drawer-nya | — | **sebagian**: `preparation-view` (fase, sign-off, bypass) nol uji | `IN PROGRESS` |
| `FE-OPR-004` | `OPS-REQ-006/009` | final read-only, addendum terpisah, `StoppedEarly`, akses dokter benar | `/execution`; `operating-room-execution-view.jsx`; `operating-room-execution.service.js`; `use-operating-room-execution.jsx` | **`operating-room-execution-addendum` (7 AC, dibuat 14 Okt)** | 14 Okt: catatan operasi `200`, addendum endpoint terpakai | **terbukti**: keempat kriteria + idempotency per perintah + status dari response | **`DONE`** |
| `FE-OPR-005` | `OPS-REQ-006/008` | recovery/handover tidak disamakan; `Completed` hanya dari response backend | `/recovery`; `operating-room-recovery-view.jsx`; `post-op-summary/` | `post-op-summary` (5 AC) — hanya drawer; `operating-room-hardening` #10 (status dari backend) | 14 Okt: anestesi `200`, recovery `Released`, handover `Sent` `200` | **sebagian**: `recovery-view` nol uji | `IN PROGRESS` |
| `FE-OPR-006` | `OPS-REQ-007` | validasi batch/serial/quantity/outcome; pending integration terlihat | `/materials`; `operating-room-material-view.jsx`; service | **nol uji** | 14 Okt: implan + serial `200`, kirim ulang idempoten | **belum** di sisi layar | `IN PROGRESS` |
| `FE-OPR-007` | `OPS-REQ-007/010` | status delivery/retry sesuai permission; tidak menampilkan payload sensitif | `/integration`; `operating-room-integration-view.jsx`; service | **nol uji** | 14 Okt: outbox 12/12 `EventId` unik | **belum** di sisi layar | `IN PROGRESS` |
| `FE-OPR-008` | `OPS-REQ-011` | filter/paging/empty/error/export bila didukung; responsive/a11y | `/reports`; `operating-room-report-view.jsx`; service | **nol uji** | 14 Okt: 3 laporan `200` dengan data | **belum** di sisi layar | `IN PROGRESS` |
| `FE-OPR-009` | semua | E2E normal/darurat/cancel/stale/`403`/`409`; tidak memakai `dataOperasi.jsx` | `tests/e2e/operating-room-flow.spec.mjs` (12 kasus, 282 baris) | `operating-room-hardening` (10, **lulus**) | **E2E dijalankan 14 Okt: 10 gagal** | **belum**: lihat di bawah | `IN PROGRESS` |

## `FE-OPR-009` — hasil eksekusi E2E sebenarnya

Dijalankan pertama kali 14 Oktober 2026 (`npx playwright test tests/e2e/operating-room-flow.spec.mjs`,
`UAT_BASE_URL=http://127.0.0.1:3000`). Backend tidak dipanggil sungguhan: spec menirukan
`**/v1/**`.

**Hasil: 10 gagal, 0 lulus.** Dua cacat ditemukan, keduanya pada perkakas uji, bukan pada layar.

**Cacat 1 — sudah diperbaiki.** Spec tidak memuat baris
`process.env.SESSION_SIGNING_SECRET ||= …` yang dimiliki setiap spec E2E lain (lihat
`blood-bank-procedure-screen.spec.mjs`). Akibatnya `createSessionAssertion` melempar
*"SESSION_SIGNING_SECRET wajib berisi minimal 32 karakter"* sebelum satu pun skenario berjalan.
Karena itu suite ini **belum pernah dapat dieksekusi sejak ditulis**. Satu baris ditambahkan
mengikuti konvensi yang sama.

**Cacat 2 — belum diperbaiki.** Sesudah perbaikan pertama, kesepuluh skenario berhenti dengan
halaman tertahan di **"Memverifikasi sesi pengguna…"**. Akarnya: aplikasi memanggil
`GET /v1/auth/me` saat bootstrap sesi, dan spec ini **lebih tua** dari mekanisme itu sehingga
tidak menirukannya. Fallthrough tiruannya mengembalikan daftar kosong, lalu `buildUserInfo` dan
`persistIdentity` menolaknya, sehingga `isAuthenticated` tidak pernah menjadi benar.

Perbaikannya bukan menebak: payload `auth/me` harus memenuhi `buildUserInfo` dan
`persistIdentity` pada `login-slice.jsx`. Bentuknya dapat dibaca dari sana, tetapi menyusunnya
tanpa memastikan setiap syarat berarti mengarang, jadi ditahan dan dilaporkan apa adanya.

Konsekuensinya jelas: **acceptance `FE-OPR-009` belum terbukti**, dan statusnya `IN PROGRESS`.

## Gap nyata yang tersisa

| Task | Gap | Dapat dikerjakan sekarang |
|---|---|---|
| `FE-OPR-001` | `loading/empty/error/stale/paging` daftar kasus nol uji | ya — uji frontend |
| `FE-OPR-002` | layar jadwal nol uji (`409`, histori, duplicate submit) | ya — uji frontend |
| `FE-OPR-003` | `preparation-view` nol uji (fase, sign-off, bypass) | ya — uji frontend |
| `FE-OPR-005` | `recovery-view` nol uji | ya — uji frontend |
| `FE-OPR-006` | layar material nol uji | ya — uji frontend |
| `FE-OPR-007` | layar integrasi nol uji | ya — uji frontend |
| `FE-OPR-008` | layar laporan nol uji | ya — uji frontend |
| `FE-OPR-009` | tiruan `GET /v1/auth/me` belum ada | ya — perkakas uji |

Delapan gap itu **seluruhnya pekerjaan uji frontend**, bukan keputusan bisnis dan bukan
dependency modul lain. Nol di antaranya menuntut perubahan source aplikasi.

## Yang sudah terbukti dan tidak perlu diulang

| Kriteria lintas task | Bukti |
|---|---|
| Nol pemakaian data dummy `dataOperasi` | `operating-room-hardening` #2 |
| Nol tulisan ke `localStorage`/`sessionStorage` | #3 |
| Nol data klinis ke console | #4 |
| Seluruh service memakai `InstanceAxios`, bukan `fetch` mentah | #5 |
| Navigasi detail memakai token route, bukan GUID | #6 |
| Token tak sah ditolak tiap layar per kasus | #7 |
| Penolakan hak akses lewat `AccessDeniedGate` | #8 |
| Tiap payload perintah membawa kunci idempotency | #9 |
| Status kasus tidak dihitung frontend | #10 |

Kesepuluh pemeriksaan itu **lulus**.
