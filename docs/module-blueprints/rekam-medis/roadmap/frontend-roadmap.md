# Rekam Medis — Frontend Delivery Roadmap

```yaml
module_id: RM-BP-001
roadmap_revision: 1
status: DRAFT
owners:
  frontend_authority: OPEN
  security_privacy: OPEN
  product_domain: OPEN
approved_by: []
input_revisions:
  interview_decisions: 4
  capability_map: 1
  frontend_architecture: 1
artifact_hashes:
  interview_decisions: sha256:2d4c37bc456a39f70d7f10e40852f5e23ba2f7f5b47b71ec0a0ed24ba248aa3c
  frontend_architecture: sha256:b7087f7bd19260f2deb7646860a02ddc354dda8377f73c9219388f9e9e1669c5
  api_contract: sha256:a20372c4b3a6b05842e733206d13b7599895b127a2c638f5533b2004e626bed8
contract_versions:
  api: 0.1.0 (draft)
source_commits:
  backend: ab37e3a2e80f0e34efe22ec0f6a8c9b90a3ae45e
  frontend: c4e2ef2a6080f3ce328d2faad79be1893ac13e22
```

Roadmap ini hanya merujuk module ID, revision, task ID, dan contract version dari roadmap
backend. Ia bukan sumber kebenaran kedua — bila terjadi perbedaan, yang berlaku adalah
`backend-roadmap.md` dan `contracts/api-contract.md`.

---

## 1. Gerbang paralel: seluruh task frontend tertahan

Ini kesimpulan terpenting dokumen ini dan harus dibaca lebih dulu.

Aturan perencanaan berbunyi: **backend dan frontend boleh berjalan paralel hanya setelah
contract version terkait berstatus `APPROVED` dan hash-nya dikunci.**

| Yang disyaratkan | Keadaan sekarang |
|---|---|
| `contracts/api-contract.md` berstatus `APPROVED` | **`draft`** |
| Hash kontrak terkunci | Hash tercatat, tetapi belum dikunci karena belum disetujui |
| Owner API menyetujui | **`OPEN`** |

**Akibatnya: sepuluh task frontend di bawah seluruhnya `TERTAHAN KONTRAK`.** Tidak satu pun
boleh dimulai sekarang.

Alasan aturan ini nyata, bukan formalitas. Bila frontend dibangun di atas bentuk payload yang
belum disetujui, dan kontraknya kemudian berubah — misalnya `AccessScope` berganti bentuk, atau
respons riwayat berubah susunannya — seluruh service, hook, dan komponen yang sudah ditulis
harus dibongkar. Pekerjaan menebak payload adalah pekerjaan yang paling mungkin terbuang.

### Yang boleh dikerjakan frontend selama menunggu

Tiga hal berikut **tidak** bergantung pada bentuk payload, sehingga aman dikerjakan:

| Pekerjaan | Mengapa aman |
|---|---|
| Menyusun brief UI untuk diajukan ke owner | Justru menutup `RM-FE-004`, `005`, `011`, `012` yang sekarang `DEV_DISCRETION` karena belum ada brief |
| Menambahkan rute rekam medis ke `tests/e2e/route-smoke.spec.mjs` | Menguji rute dapat dicapai, bukan isinya |
| Mempelajari pola `use-doctor-cppt.js` sebagai contoh | Membaca, bukan menulis |

Menyusun brief UI adalah yang paling bernilai di antara ketiganya. Empat butir `DEV_DISCRETION`
pada arsitektur frontend muncul semata-mata karena belum ada brief yang disetujui — bukan
karena keputusannya memang diserahkan ke developer.

---

## 2. Ringkasan status seluruh task

| Milestone | Task | Status | Tertahan oleh |
|---|---|---|---|
| F0 | `FE-00` | `TERTAHAN KONTRAK` | API `0.1.0` masih `draft` |
| F1 | `FE-01`, `FE-02` | `TERTAHAN KONTRAK` + `BE-14`, `BE-11` | Kontrak dan backend |
| F2 | `FE-03`, `FE-04` | `TERTAHAN KONTRAK` + `BE-04`, `BE-06` | Kontrak dan backend |
| F3 | `FE-05`, `FE-06` | `TERTAHAN KONTRAK` + `BE-12`, `BE-09` | Kontrak dan backend |
| F4 | `FE-07`, `FE-08` | `TERTAHAN KONTRAK` | Kontrak |
| F5 | `FE-09` | `TERTAHAN KONTRAK` + seluruh task pendahulu | Kontrak dan backend |

**Denominator: 10 task, seluruhnya tertahan. Tidak ada yang berstatus `SIAP`.**

---

## 3. Milestone F0 — Fondasi

### `FE-00` — Lapisan service dan hook

| Field | Isi |
|---|---|
| **Task ID** | `FE-00` |
| **Status** | `TERTAHAN KONTRAK` |
| **Outcome** | Frontend punya cara memanggil API rekam medis, dengan penanganan galat yang seragam |
| **Trace** | API contract `0.1.0`; arsitektur frontend bagian 5 |
| **Reuse** | Pola `src/lib/services/health-services/clinical-management/`; `InstanceAxios`; pembungkus `unwrapApiResponse` |
| **Scope** | `src/lib/services/health-services/medical-record-management/` empat berkas service; `src/lib/hooks/health-services/medical-record-management/` tiga hook |
| **Dependency** | Kontrak API `APPROVED`; `BE-14` tersedia |
| **Acceptance criteria** | 1) Seluruh endpoint pada kontrak punya fungsi service. 2) Galat `503` ditangani sebagai pesan "berkas tidak dapat dibuka saat ini", bukan pesan teknis. 3) Galat `409` ditangani sebagai pesan nomor rekam medis digabungkan |
| **Verification** | Uji unit service dengan jawaban tiruan |
| **Risk/blocker** | Bila kontrak berubah setelah task ini dikerjakan, seluruh berkasnya perlu disesuaikan |
| **DoD** | Service dan hook lengkap; penanganan `503` dan `409` terbukti |

---

## 4. Milestone F1 — Slice minimum: berkas rekam medis dapat dibaca

### `FE-01` — Layar berkas rekam medis pasien

| Field | Isi |
|---|---|
| **Task ID** | `FE-01` |
| **Status** | `TERTAHAN KONTRAK` |
| **Outcome** | Petugas dapat melihat seluruh riwayat klinis seorang pasien lintas kunjungan dalam satu tempat, tanpa membuka kunjungan satu per satu |
| **Trace** | `RM-DEC-002`, `RM-DEC-013`, `RM-DEC-018`; `RM-FE-001`, `RM-FE-006`, `RM-FE-008`, `RM-FE-009` |
| **Reuse** | Pola pemuatan bertahap per bulan pada `use-doctor-cppt.js:372`; komponen tabel yang sudah ada |
| **Scope** | `src/app/health-services/medical-record-management/medical-records/page.jsx`; komponen tampilannya |
| **Dependency** | `FE-00`; `BE-14` |
| **Acceptance criteria** | 1) Dokumen dari beberapa kunjungan tampil urut waktu dalam satu daftar. 2) Status keutuhan dan status alur kerja **dapat dibedakan pembaca**. 3) Keterangan bahwa baru CPPT yang tunduk aturan keutuhan terlihat. 4) Keterangan bahwa label kerahasiaan belum membatasi akses terlihat. 5) Bila satu sumber gagal dimuat, bagian lain tetap tampil dan yang gagal **ditandai jelas**. 6) `409` menampilkan pesan nomor pengganti, **bukan** riwayat sebagian |
| **Verification** | `AT-RM-09`, `AT-RM-32`, `AT-RM-39`, `AT-RM-42` |
| **Risk/blocker** | **Risiko tertinggi di frontend:** riwayat yang tampil tidak lengkap tanpa peringatan akan dibaca sebagai riwayat lengkap, dan keputusan klinis dapat diambil di atasnya. Ditutup acceptance criteria nomor 5 |
| **DoD** | Enam acceptance criteria terbukti; `RM-FE-001`, `006`, `008`, `009` terpenuhi |

### `FE-02` — Kotak isian keperluan akses

| Field | Isi |
|---|---|
| **Task ID** | `FE-02` |
| **Status** | `TERTAHAN KONTRAK` |
| **Outcome** | Petugas yang membuka rekam medis pasien di luar rawatannya diminta keperluan lebih dulu, dan tahu bahwa aksesnya dicatat |
| **Trace** | `RM-DEC-005`, `RM-DEC-016`; `RM-FE-003` |
| **Reuse** | Komponen modal atau drawer yang sudah ada — bentuknya `DEV_DISCRETION` |
| **Scope** | Komponen penghalang; dipakai `FE-01` dan `FE-04` |
| **Dependency** | `FE-00`; `BE-11`, `BE-09` |
| **Acceptance criteria** | 1) Pilihan keperluan diambil dari master, **tidak** ditulis tetap di kode. 2) Kotak alasan bebas muncul bila keperluan yang dipilih menuntutnya. 3) **Isi rekam medis tidak terlihat sedikit pun sebelum keperluan diisi** — termasuk tidak boleh dimuat di belakang layar lalu ditutupi lapisan buram. 4) Pengguna diberi tahu bahwa akses ini dicatat dan akan ditinjau |
| **Verification** | `AT-RM-41` |
| **Risk/blocker** | **Aturan nomor 3 mudah dilanggar tanpa disadari.** Memuat data lalu menutupinya secara visual berarti isi sudah berpindah ke perangkat pengguna dan dapat dilihat lewat alat pengembang peramban. Penghalangnya harus terjadi sebelum permintaan dikirim |
| **DoD** | Empat acceptance criteria terbukti; pemeriksaan lalu lintas jaringan membuktikan tidak ada permintaan isi sebelum keperluan diisi |

---

## 5. Milestone F2 — Menandatangani dan mengoreksi

### `FE-03` — Layar catatan saya yang belum ditandatangani

| Field | Isi |
|---|---|
| **Task ID** | `FE-03` |
| **Status** | `TERTAHAN KONTRAK` |
| **Outcome** | Dokter dan perawat dapat menemukan catatannya sendiri yang belum ditandatangani, lalu menandatanganinya |
| **Trace** | `RM-DEC-003`, `RM-DEC-021` |
| **Reuse** | Pola daftar dan tabel yang sudah ada |
| **Scope** | `src/app/health-services/medical-record-management/my-unsigned-notes/page.jsx` |
| **Dependency** | `FE-00`; `BE-04` |
| **Acceptance criteria** | 1) Daftar hanya memuat catatan milik pengguna. 2) Menandatangani tidak meminta kata sandi maupun sidik jari. 3) Tombol terkunci setelah ditekan agar tidak terkirim ganda. 4) Setelah ditandatangani, catatan hilang dari daftar |
| **Verification** | `AT-RM-18` |
| **Risk/blocker** | Bila layar ini tidak ada, catatan yang lupa ditandatangani tidak dapat ditemukan, dan seluruhnya berakhir `LockedUnsigned`. Layar ini yang membuat `RM-DEC-003` bekerja sebagaimana dimaksud |
| **DoD** | Empat acceptance criteria terbukti |

### `FE-04` — Tampilan dan pembuatan addendum

| Field | Isi |
|---|---|
| **Task ID** | `FE-04` |
| **Status** | `TERTAHAN KONTRAK` |
| **Outcome** | Koreksi atas catatan yang sudah terkunci tampil menempel pada catatan aslinya, sehingga pembaca melihat keduanya beserta urutannya |
| **Trace** | `RM-DEC-004`, `RM-DEC-020`; `RM-FE-002`, `RM-FE-010` |
| **Reuse** | `FE-01` |
| **Scope** | Komponen addendum di dalam layar berkas rekam medis |
| **Dependency** | `FE-01`; `BE-06` |
| **Acceptance criteria** | 1) Addendum tampil **menempel** pada dokumen induknya, bukan sebagai entri terpisah. 2) Isi asli tetap terbaca utuh. 3) Tombol addendum hanya muncul bila endpoint pemeriksa kewenangan menyatakan berhak. 4) Bila tidak berhak, alasannya dijelaskan. 5) **Tombol terkunci setelah ditekan** |
| **Verification** | `AT-RM-40` |
| **Risk/blocker** | **Addendum tidak dapat diubah maupun dihapus.** Tekan ganda menghasilkan dua koreksi kembar yang menempel selamanya pada rekam medis. Pencegahannya hanya ada di sini |
| **DoD** | Lima acceptance criteria terbukti; percobaan tekan ganda tidak menghasilkan addendum kembar |

---

## 6. Milestone F3 — Tinjauan dan master

### `FE-05` — Layar tinjauan akses

| Field | Isi |
|---|---|
| **Task ID** | `FE-05` |
| **Status** | `TERTAHAN KONTRAK` |
| **Outcome** | Unit rekam medis dapat memeriksa akses yang perlu ditinjau, sehingga jejak akses berguna alih-alih hanya menumpuk |
| **Trace** | `RM-DEC-005` |
| **Reuse** | Pola daftar dengan penyaring yang sudah ada |
| **Scope** | `src/app/health-services/medical-record-management/access-review/page.jsx` |
| **Dependency** | `FE-00`; `BE-12` |
| **Acceptance criteria** | 1) Hanya baris bertanda perlu ditinjau yang muncul pada antrean. 2) Nama pengakses, pasien, waktu, keperluan, dan alasan terlihat. 3) Menandai sudah ditinjau memerlukan catatan tinjauan |
| **Verification** | `AT-RM-29` |
| **Risk/blocker** | **Perhatian privasi:** layar ini memuat `AccessReason` yang bertanda sensitif. Hak aksesnya harus lebih sempit daripada hak baca rekam medis |
| **DoD** | Tiga acceptance criteria terbukti; batasan hak akses terpasang |

### `FE-06` — Master keperluan akses

| Field | Isi |
|---|---|
| **Task ID** | `FE-06` |
| **Status** | `TERTAHAN KONTRAK` |
| **Outcome** | Unit rekam medis dapat mengubah daftar keperluan akses sendiri, tanpa meminta perubahan kode |
| **Trace** | Arsitektur backend bagian 9 |
| **Reuse** | Pola layar master data pada `src/components/view/health-services/master-data/`. **Tidak perlu pola baru** |
| **Scope** | Layar master mengikuti pola yang sudah ada |
| **Dependency** | `FE-00`; `BE-09` |
| **Acceptance criteria** | 1) Tambah, ubah, dan aktifkan atau nonaktifkan berjalan. 2) Penanda "menuntut alasan bebas" dapat diatur |
| **Verification** | Uji rute dapat dicapai; pemeriksaan manual |
| **Risk/blocker** | Rendah. Mengikuti pola yang sudah terbukti pada 24 layar master lain |
| **DoD** | Layar berjalan mengikuti pola master data yang ada |

---

## 7. Milestone F4 — Navigasi dan kelengkapan

### `FE-07` — Menu dan rute

| Field | Isi |
|---|---|
| **Task ID** | `FE-07` |
| **Status** | `TERTAHAN KONTRAK` |
| **Outcome** | Petugas dapat mencapai layar rekam medis dari menu |
| **Trace** | `RM-CAP-008`; `RM-FE-004` |
| **Reuse** | `src/utils/menu-sidebar/menu-items.jsx` |
| **Scope** | Penambahan entri menu; pendaftaran rute pada uji asap rute |
| **Dependency** | `FE-01` |
| **Acceptance criteria** | 1) Menu rekam medis dapat dicapai. 2) Menu hanya muncul bagi peran yang berhak. 3) Rute baru terdaftar pada `tests/e2e/route-smoke.spec.mjs` |
| **Verification** | `tests/e2e/route-smoke.spec.mjs` |
| **Risk/blocker** | Catatan: kunci `menuLaboratorium` dan sejenisnya pada `left-sidebar-menu-handle.jsx:6-19` **bukan** definisi menu, melainkan daftar nama kelompok menu bersarang. Menu sesungguhnya ada di `menu-items.jsx`. Jangan tertukar |
| **DoD** | Menu dan rute berjalan; uji asap rute lulus |

### `FE-08` — Penanganan keadaan tidak biasa

| Field | Isi |
|---|---|
| **Task ID** | `FE-08` |
| **Status** | `TERTAHAN KONTRAK` |
| **Outcome** | Pengguna mendapat penjelasan yang dapat dipahami ketika terjadi gangguan, bukan pesan galat teknis |
| **Trace** | Arsitektur frontend bagian 4; `RM-FE-012` |
| **Reuse** | Pola penanganan galat yang sudah ada |
| **Scope** | Seluruh layar rekam medis |
| **Dependency** | `FE-01` sampai `FE-06` |
| **Acceptance criteria** | 1) Keadaan memuat ditampilkan per bagian, bukan menutup seluruh layar. 2) "Belum ada riwayat" dibedakan dari "penyaringan tidak menemukan apa pun". 3) Gagal sebagian **ditandai jelas**, tidak disembunyikan. 4) `503` ditampilkan sebagai pesan yang dapat dipahami. 5) `PrivateNote` **tidak** disimpan sementara di sisi klien |
| **Verification** | Pemeriksaan manual tiap keadaan; pemeriksaan penyimpanan peramban untuk nomor 5 |
| **Risk/blocker** | Aturan nomor 3 adalah yang paling penting di seluruh roadmap frontend. Riwayat tidak lengkap yang tampil tanpa peringatan dapat menjadi dasar keputusan klinis yang keliru |
| **DoD** | Lima acceptance criteria terbukti |

---

## 8. Milestone F5 — Kesiapan

### `FE-09` — Uji antarmuka dan kesiapan

| Field | Isi |
|---|---|
| **Task ID** | `FE-09` |
| **Status** | `TERTAHAN KONTRAK` |
| **Outcome** | Empat aturan antarmuka yang berasal dari keputusan keamanan dan klinis terbukti berjalan, bukan hanya dinyatakan |
| **Trace** | `AT-RM-39` sampai `AT-RM-42` |
| **Reuse** | `tests/e2e/` yang sudah ada |
| **Scope** | Berkas uji antarmuka baru |
| **Dependency** | `FE-01` sampai `FE-08` |
| **Acceptance criteria** | Empat uji `AT-RM-39` sampai `AT-RM-42` ada dan lulus |
| **Verification** | Keluaran perintah uji |
| **Risk/blocker** | Repository hanya punya 4 berkas uji, tidak satu pun menyentuh alur klinis. Belum ada padanan yang bisa dicontoh untuk pengujian alur seperti ini |
| **DoD** | Empat uji ada dan lulus |

---

## 9. Ruang `DEV_DISCRETION` dan invariant yang tetap mengikat

Empat butir berikut diserahkan ke developer **hanya karena belum ada brief UI yang disetujui**,
bukan karena memang kewenangan developer:

| Butir | Ruang bebas |
|---|---|
| `RM-FE-004` | Bentuk navigasi: menu, rute, tab, modal, atau drawer |
| `RM-FE-005` | Tata letak, warna, ikon, komponen tabel |
| `RM-FE-011` | Cara memuat bertahap: gulir tanpa batas, tombol muat lagi, atau halaman |
| `RM-FE-012` | Bentuk penanda dokumen gagal dimuat sebagian |

Delapan butir berikut **tidak** boleh diputuskan developer, karena berasal dari keputusan
keamanan, privasi, atau invariant klinis:

| Butir | Yang mengikat |
|---|---|
| `RM-FE-001` | Penanda status keutuhan wajib terlihat |
| `RM-FE-002` | Addendum wajib menempel pada dokumen induknya |
| `RM-FE-003` | Isian keperluan wajib mendahului tampilnya isi, termasuk mendahului permintaan ke server |
| `RM-FE-006` | Keterangan bahwa label kerahasiaan belum membatasi akses |
| `RM-FE-007` | `PrivateNote` tidak tampil pada tampilan rutin |
| `RM-FE-008` | Dua status harus dapat dibedakan pembaca |
| `RM-FE-009` | Keterangan bahwa baru CPPT yang tunduk aturan keutuhan |
| `RM-FE-010` | Tombol yang tidak berhak ditekan tidak ditampilkan |

Invariant yang berlaku pada seluruh ruang `DEV_DISCRETION`: apa pun bentuk visual yang dipilih,
**tidak boleh** menyembunyikan keberadaan penanda status, penanda gagal sebagian, maupun
keterangan cakupan. Kebebasan berlaku pada bentuknya, bukan pada ada atau tidaknya.

---

## 10. Yang sengaja tidak masuk roadmap ini

| Yang tidak dikerjakan | Alasan |
|---|---|
| Layar kelengkapan berkas, koding, resume medis, peminjaman | Cakupan 4 sampai 8, rilis berikutnya |
| Portal pasien untuk melihat rekam medisnya sendiri | Di luar scope. `SelfServices` tidak memuat kemampuan klinis apa pun |
| Penyuntingan catatan klinis dari layar rekam medis | Melanggar `RM-DEC-001` |
| Pengunduhan seluruh riwayat sebagai satu berkas | Menciptakan salinan rekam medis di luar sistem tanpa jejak. Perlu keputusan pelepasan informasi lebih dulu |
