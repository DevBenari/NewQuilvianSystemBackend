# Rawat Jalan Billing — Blueprint Manifest

| Field | Value |
|---|---|
| `blueprint_id` | `RJ-BIL-BP-001` |
| `module_name` | Dokter / Rawat Jalan Billing |
| `module_slug` | `rawat-jalan` |
| `module_prefix` | `RJ-BIL` |
| `revision` | `21` |
| `status` | `PARTIAL` |
| `current_phase` | `RJ-BIL-PH-008` — Delivery Planning |
| `created_at` | `2026-08-20T15:06:30+07:00` |
| `updated_at` | `2026-08-28T14:02:11+07:00` |
| `last_verified_at` | `2026-08-28T14:02:11+07:00` |
| `backend_source_sha` | `6b25e6049e60e055593968abe463262b59842527` cabang `sukmagp`; working tree `RJ-BIL-BE-002`, `RJ-BIL-BE-003`, `RJ-BIL-BE-006`, `RJ-BIL-BE-007`, dan remediasi penamaan QBE belum di-commit |
| `frontend_source_sha` | `ab4bd836e05c72d0679e02899258f3773f3869a2` |
| `skill_suite_version` | `1.0.0-rc2` |
| `input_revision_hash` | `decisions:sha256:18D9B0CFEF4EDA1F22170FC8EA7FCDC84A317AC8B1C7B907F6BA8B4B4435F86D; capability:sha256:D1CB1D052474FA96F0BE801F7CEA277AEB0604A9969247B7323F82D23F5152B7` |
| `decision_revision` | `14` |
| `contract_versions` | `RJ-BIL-CONTRACT-001@1.0.0 (OWNER_APPROVED)` |
| `active_dependency_ids` | `RJ-BIL-DEP-001` s.d. `RJ-BIL-DEP-009` |
| `active_roadmap_revision` | `1` |
| `supersedes` | `null` |
| `domain_architecture_revision` | `1` |
| `domain_architecture_readiness` | `DOMAIN_ARCHITECTURE_PARTIAL`; core internal/manual siap independen |
| `owners` | Product/Domain owner; Billing/Revenue Cycle; API authority; Security/Privacy; Frontend authority |
| `approved_by` | `User-provided approval authority` |
| `approved_at` | `2026-08-21` |

## Artifact hashes

Hash artefak target dihitung pada `2026-08-20` untuk mendeteksi drift. Semua artefak desain masih
berstatus `draft` dan belum menjadi izin implementasi.

| Artifact | SHA-256 |
|---|---|
| `02-backend-architecture.md` | `524AF2A661A77092092A9A896571415DA0FA45576CDB5D01B6C624DCA3FFA22E` |
| `03-frontend-architecture.md` | `05304BDFC6323930E80516AC12A73A7CB34FD3A0378C0449EB560E58BADFA4E8` |
| `hospital-domain-architecture.md` | `E7B0B0F08DB9CFB2B7FEF6727F705FA09ED0720CC6FEE32F7EE0C1DD8EADF96E` |
| `contracts/api-contract.md` | `9508986836A537DD88A664F052003AE1A8509EE0555DD6DC2F3F83AB6B871FD6` |
| `contracts/state-transition-matrix.md` | `1688209BFEEAFA3F5A48A70376FCE4CA291239F189343FFF615D05C2C39B5807` |
| `contracts/validation-matrix.md` | `8C3FF0A302BAB10BC6DC904630F3FE0BD2DA675F1DE2491586F08CF596209E53` |
| `contracts/integration-contract.md` | `5743FC7B31A27500360FDE9E3EA61856D1CB4290539B07CDA8640F6F9E112DB6` |
| `contracts/permission-audit-matrix.md` | `1424ADE6BB5084C8A77105477C65B347959C6C17570A88E2E9663C38C23FF093` |
| `testing/acceptance-test-matrix.md` | `FEC2E3816EF086540FB85EAB9242A559906BF65154B1DC311068A5C999840EF6` |
| `owner-review-checklist.md` | `review artifact; hash dihitung setelah owner mengisi record approval` |
| `roadmap/backend-roadmap.md` | `B6CC1AC74D61A779FAA08CF587BF0BBFB8731079C2744F426F87BD948B208343` |
| `roadmap/frontend-roadmap.md` | `EDE0FD3A0B26754BF5626A392587664DD2C96C0F5FAD279EA2F6B74C14001D55` |
| `roadmap/requirement-traceability.md` | `B032A1DCF177EC1BA0F3B31D6283376CFFC824298F4A0B893918707CBE0B1615` |

Catatan verifikasi `2026-08-28` (revision `21`) — **`RJ-BIL-FE-002` selesai untuk Resep,
Tindakan, dan Laboratorium**. `next build` lulus exit `0` dengan kedua route Billing terbukti
dihasilkan, `88` test unit lulus tanpa satu pun gagal, dan lint berkas Billing bersih.

**Bagian Radiologi sengaja tidak dikerjakan** dan tetap ⛔. Radiologi belum terdaftar pada
`BillingSourceContract` backend, sehingga faktanya tidak akan pernah sampai ke layar. Ketiadaannya
**diumumkan di layar** alih-alih didiamkan: baris radiologi yang tidak muncul berarti *belum
diketahui*, bukan *tidak ada pemeriksaan*. Membiarkannya senyap akan membuat layar berbohong
secara pasif.

Dua temuan baru dicatat pada `roadmap/requirement-traceability.md` bagian 3, dan **tidak satu pun
diperbaiki dari task ini** karena keduanya milik modul lain:

| Temuan | Mengapa penting |
|---|---|
| `PrescriptionResponse.paymentStatus` masih mengirim nilai `Lunas` | `RJ-BIL-BE-002` sudah mencabut kewenangan finansial modul klinis dan menghapus endpoint yang **menulis** kolom itu — yang **membaca** belum. Siapa pun yang mengikat kolom itu ke badge status akan menampilkan pesanan klinis sebagai lunas, tanpa satu pun galat yang memberi tahu bahwa itu salah. Layar `FE-002` menjinakkannya untuk dirinya sendiri, bukan untuk layar lain |
| `GET /lab-orders` tanpa satu pun parameter penyaring | Tidak ada `encounterId`, tidak ada paginasi; seluruh pesanan laboratorium rumah sakit terkirim sekaligus. `FE-002` terpaksa menyaring per kunjungan di sisi klien. Berjalan sekarang; berhenti berjalan seiring pertumbuhan data. Yang harus berubah adalah endpoint-nya |

Satu butir Definition of Done `RJ-BIL-FE-002` **sengaja dibiarkan terbuka**: *"Batas klinis–finansial
ditinjau domain owner."* Implementasinya selesai dan teruji, tetapi tinjauan domain owner hanya
dapat dilakukan manusia dan tidak boleh ditandai selesai dari sini.

Dua hash berubah: `roadmap/frontend-roadmap.md` dan `roadmap/requirement-traceability.md`.
Sepuluh artefak lain diverifikasi ulang dan **cocok**.

Laporan task: [fe-rj-bil-002-batas-klinis-dan-finansial.md](task/report/frontend/fe-rj-bil-002-batas-klinis-dan-finansial.md).

---

Catatan verifikasi `2026-08-28` (revision `20`) — **`RJ-BIL-FE-001` selesai**. Task frontend
pertama modul ini dikerjakan di bawah `RJ-BIL-DEC-013` dan terbukti: `next build` lulus dengan
exit `0`, route `[encounterId]` terbukti dihasilkan pada `app-path-routes-manifest.json`, `67` test
unit lulus tanpa satu pun gagal, dan lint berkas Billing bersih.

Dua hash berubah: `roadmap/frontend-roadmap.md` dan `roadmap/requirement-traceability.md`.
Sepuluh artefak lain diverifikasi ulang dan **cocok**.

Tiga hal yang perlu diketahui pembaca berikutnya, dan **sengaja tidak dirapikan** dari task itu:

| Hal | Mengapa dibiarkan |
|---|---|
| `source_commits.frontend` masih `ab4bd836`, tertinggal **11 commit** dari `HEAD` `bd31dc99` | Menebak snapshot mana yang benar adalah wewenang pemilik frontend. Klaim terpenting yang bergantung padanya — `RJ-BIL-CAP-020` *Missing* — sudah diverifikasi ulang langsung ke pohon saat ini dan **masih benar** |
| Baris **Dependency** `RJ-BIL-FE-001` kurang menyebut `RJ-BIL-BE-007` | Scope task menuntut `processing outcome`, yang hanya tersedia pada endpoint milik `BE-007`. Dicatat sebagai delta pada roadmap dan laporan task; melengkapinya adalah wewenang pemilik roadmap |
| `Component render test` belum ada | Harness project memakai `node --test` tanpa `@testing-library`, sehingga render test memang **belum mungkin** ditulis. Ini batas alat, bukan pilihan; penutupannya adalah cakupan `RJ-BIL-FE-007` |

`RJ-BIL-CAP-020` kini **tertutup sebagian**: layar folio baca sudah ada, sedangkan payer split dan
correction status masih tidak punya satu pun consumer. `RJ-BIL-CAP-021` juga bergerak dari nihil
menjadi `29` test unit, dengan render test tetap kosong.

Laporan task: [fe-rj-bil-001-baca-tagihan-satu-kunjungan.md](task/report/frontend/fe-rj-bil-001-baca-tagihan-satu-kunjungan.md).

---

Catatan verifikasi `2026-08-28` (revision `19`) — **wewenang tulis frontend diberikan**.
`RJ-BIL-DEC-013` menaikkan `IMPLEMENTATION_AUTHORITY` roadmap frontend dari `NOT_GRANTED` menjadi
`GRANTED`, dan `BUILDER_EXECUTION` menjadi `AUTHORIZED` untuk **empat** task saja —
`RJ-BIL-FE-001`, `RJ-BIL-FE-002` bagian Lab, `RJ-BIL-FE-004`, dan `RJ-BIL-FE-005`.

Tiga task sisanya sengaja **tidak** ikut dinaikkan. `RJ-BIL-FE-003`, `RJ-BIL-FE-006`, dan
`RJ-BIL-FE-007` tetap `NOT_AUTHORIZED` karena endpoint pasangannya belum ada sama sekali;
menaikkannya bersama yang lain hanya akan membuat task tanpa backend tampak siap dikerjakan.
Bagian Radiologi pada `RJ-BIL-FE-002` juga dikecualikan selama `RJ-BIL-BE-004` terblokir.

Yang **tidak** diberikan keputusan ini: commit, push, merge, deployment, perubahan backend, dan
aktivasi `RJ-BIL-DEP-009`. `Frontend authority` atas route, menu, dan bentuk komponen pada
`03-frontend-architecture.md` bagian `7` tetap `OPEN`, begitu pula `Security/Privacy` — keduanya
sengaja dibiarkan terbuka dan tetap menjadi syarat sebelum aktivasi production.

Tiga hash berubah karena keputusan ini: `00-interview-decisions.md` (`decision_revision` `13` →
`14`), `roadmap/frontend-roadmap.md`, dan `roadmap/requirement-traceability.md` — yang blocker
*"wewenang tulis frontend belum diberikan"*-nya ditandai tertutup, bukan dihapus, supaya jejak
bahwa blocker itu pernah ada tidak hilang. Sembilan artefak lain diverifikasi ulang dan **cocok**.
`MODULE-STATUS.md` ikut dimutakhirkan tetapi tidak masuk tabel hash ini.

Satu hal yang perlu diketahui pembaca berikutnya dan **sengaja tidak saya sentuh**:
`MODULE-STATUS.md` mencatat `Frontend source SHA` `32db4acb…` sedangkan manifest dan kedua roadmap
memakai `ab4bd836…`. Selisih itu sudah ada sebelum keputusan ini dan tidak diciptakan olehnya.
Menyamakannya secara sepihak berarti menebak snapshot mana yang benar; yang berwenang menyatakan
itu adalah pemilik frontend.

---

Catatan verifikasi `2026-08-28` (revision `18`) — **penyelarasan artefak governance**. Seluruh
tiga belas hash dihitung ulang. Sembilan artefak desain dan kontrak cocok tanpa perubahan; ketiga
artefak roadmap berubah dan hash-nya diperbarui di sini, begitu pula `input_revision_hash` untuk
decisions.

Pemicunya: setelah `RJ-BIL-BE-006` selesai `2026-08-27`, sebagian artefak dimutakhirkan dan
sebagian tertinggal, sehingga beberapa dokumen bertentangan satu sama lain. Yang diselaraskan:

| Artefak | Yang melenceng | Perbaikan |
|---|---|---|
| `00-interview-decisions.md` | Header masih `Revision 10` padahal `RJ-BIL-DEC-011` dan `RJ-BIL-DEC-012` sudah tercatat dan `approved` di dalamnya | Header disamakan dengan `decision_revision` yang sudah dicatat manifest, yaitu `13`. Isi keputusan **tidak disentuh** |
| `MODULE-STATUS.md` | `IMPLEMENTATION_AUTHORITY` dan `BUILDER_EXECUTION` belum memuat `RJ-BIL-BE-006`; `Evidence state` masih menyebut `22` test, `3` migration, dan `88` migration terdaftar; **`Next recommended task` masih memakai urutan dependency yang sudah dikoreksi `RJ-BIL-DEC-008`** | Ketiganya dimutakhirkan. Urutan lama diberi catatan koreksi eksplisit, dan langkah berikutnya disusun ulang menurut apa yang benar-benar menahan modul |
| `roadmap/requirement-traceability.md` | Masih menyatakan `IMPLEMENTATION_AUTHORITY NOT_GRANTED`, `BUILDER_EXECUTION NOT_AUTHORIZED`, dan *"tidak ada test project pada snapshot"* | Ditulis ulang mengikuti bentuk kedua roadmap; kolom **Keadaan** dan **Governance** dipisahkan supaya ✅ tidak terbaca sebagai izin production |
| `roadmap/backend-roadmap.md` | Ringkasan bagian 0 tertinggal — `4 dari 9`, `111` test, `RJ-BIL-BE-006` terblokir — bertentangan dengan isi dokumennya sendiri | Disamakan menjadi `5 dari 9` dan `157` test |
| `roadmap/frontend-roadmap.md` | `RJ-BIL-FE-004` masih tercatat terblokir padahal `RJ-BIL-BE-006` sudah selesai | Enam tempat disamakan |
| `testing/readiness-report.md` | Catatan penyeliaan masih menyebut `111` test dan `4` dari `9` task | Catatan dimutakhirkan. **Badan laporan sengaja tidak ditulis ulang** — ia potret audit `2026-08-24`, dan verdict `NOT_READY` tetap berlaku |

Kedua roadmap juga memakai bentuk penyajian baru sejak `2026-08-27`: dari satu tabel sebelas
kolom menjadi struktur bagian bernomor dengan tabel `Field / Isi` vertikal per task, mengikuti
bentuk `rawat-inap`. Cakupan, acceptance criteria, dependency, dan kontrak **tidak berubah**;
`active_roadmap_revision` karena itu tetap `1`.

`current_phase` dinaikkan `RJ-BIL-PH-008` → `RJ-BIL-PH-009` agar cocok dengan `MODULE-STATUS.md`,
yang sejak revision `18` sudah mencatat `PH-008` sebagai fase selesai. `backend_source_sha`
dilengkapi: working tree yang belum di-commit mencakup `RJ-BIL-BE-002`, `003`, `006`, `007`, dan
remediasi penamaan QBE — bukan hanya `RJ-BIL-BE-003`.

Baris `owner-review-checklist.md` sengaja tidak memuat hash; isinya catatan, dan tetap demikian
sampai owner mengisi record approval.

Arsitektur domain revision `1` sudah tersedia. Fase berikutnya adalah `design-business-module`
untuk slice core internal/manual yang siap. Aktivasi adapter eksternal tetap terblokir oleh
`RJ-BIL-DEP-009`.
