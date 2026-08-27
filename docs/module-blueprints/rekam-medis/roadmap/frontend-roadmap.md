# Rekam Medis — Frontend Delivery Roadmap

```yaml
module_id: RM-BP-001
roadmap_revision: 2
status: DRAFT
owners:
  frontend_authority: Yoga Aji Pratama
  security_privacy: Yoga Aji Pratama
  product_domain: Yoga Aji Pratama
approved_by: [Yoga Aji Pratama]
approved_at: 2026-08-27
input_revisions:
  interview_decisions: 4
  capability_map: 1
  frontend_architecture: 2
artifact_hashes:
  interview_decisions: sha256:2d4c37bc456a39f70d7f10e40852f5e23ba2f7f5b47b71ec0a0ed24ba248aa3c
  frontend_architecture: sha256:3c2662e502668f3b57c92921d38e7f5b57ff79ac48f9a781b7b85b775c349b1f
  api_contract: sha256:a20372c4b3a6b05842e733206d13b7599895b127a2c638f5533b2004e626bed8
contract_versions:
  api: 0.1.0 (approved 2026-08-27)
source_commits:
  backend: ab37e3a2e80f0e34efe22ec0f6a8c9b90a3ae45e
  frontend: c4e2ef2a6080f3ce328d2faad79be1893ac13e22
```

Roadmap ini hanya merujuk module ID, revision, task ID, dan contract version dari roadmap
backend. Ia bukan sumber kebenaran kedua — bila terjadi perbedaan, yang berlaku adalah
`backend-roadmap.md` dan `contracts/api-contract.md`.

---

## 1. Gerbang paralel: TERBUKA sejak 27 Agustus 2026

Ini kesimpulan terpenting dokumen ini dan harus dibaca lebih dulu.

Aturan perencanaan berbunyi: **backend dan frontend boleh berjalan paralel hanya setelah
contract version terkait berstatus `APPROVED` dan hash-nya dikunci.**

| Yang disyaratkan | Keadaan sekarang |
|---|---|
| `contracts/api-contract.md` berstatus `APPROVED` | **Terpenuhi** — `approved` sejak 27 Agustus 2026 |
| Hash kontrak terkunci | **Terpenuhi** — dikunci bersama pengesahan |
| Owner API menyetujui | **Terpenuhi** — Yoga Aji Pratama (`RM-DEC-028`) |

**Akibatnya: sepuluh task frontend di bawah tidak lagi `TERTAHAN KONTRAK`.** Yang menahan
sekarang tinggal dependency antar-task dan ketersediaan endpoint backend-nya.

Alasan aturan ini nyata, bukan formalitas. Bila frontend dibangun di atas bentuk payload yang
belum disetujui, dan kontraknya kemudian berubah — misalnya `AccessScope` berganti bentuk, atau
respons riwayat berubah susunannya — seluruh service, hook, dan komponen yang sudah ditulis
harus dibongkar.

### Dua delta yang ikut disahkan, dan wajib diperhatikan frontend

Keduanya diterapkan `BE-14` dan dirinci pada `contracts/api-contract.md` bagian 2:

| Delta | Dampak pada frontend |
|---|---|
| Balasan `/timeline` dibungkus `MedicalRecordTimelineResponse` | Isi halaman dibaca dari **`data.page.items`**, bukan `data.items` |
| Field `access` pada seluruh balasan endpoint berkas | Tersedia keterangan jenis akses dan apakah pembukaannya akan ditelaah — dipakai `FE-02` |

Selubung riwayat juga membawa `failedSources`, `isTruncated`, dan `isComplete`. **Ketiganya
bukan hiasan**: acceptance criteria `FE-01` nomor 5 menuntut sumber yang gagal dimuat ditandai
jelas, dan inilah datanya.

### Keadaan backend saat gerbang dibuka

Seluruh pekerjaan kode backend selesai. Yang perlu diketahui frontend:

| Hal | Keadaan |
|---|---|
| Endpoint berkas rekam medis | **Tersedia** — lima endpoint, lihat api-contract bagian 2 |
| Master keperluan akses (`MstMedicalRecordAccessPurpose`) | **Masih kosong** — menunggu SOP. Selama kosong, pembukaan pasien di luar rawatan **selalu** ditolak |
| Penanda master kosong | `/filters/metadata` mengembalikan `isAccessPurposeMasterEmpty` beserta peringatannya |

Butir kedua langsung menyentuh `FE-02`: layar keperluan akses akan menampilkan daftar pilihan
kosong sampai master itu terisi. **Keadaan itu wajib dinyatakan di layar**, bukan tampil sebagai
daftar kosong tanpa penjelasan yang membuat pengguna mengira sistemnya rusak.

---

## 2. Ringkasan status seluruh task

Status per 27 Agustus 2026, setelah pengesahan `RM-DEC-028` dan selesainya seluruh kode backend.

| Milestone | Task | Status | Keterangan |
|---|---|---|---|
| F0 | `FE-00` | **`SIAP`** | Lapisan service dan hook. **Kerjakan lebih dulu** — seluruh task lain bergantung padanya |
| F1 | `FE-01` | **`SIAP`** setelah `FE-00` | Backend-nya sudah tersedia (`BE-14`) |
| F1 | `FE-02` | **`SIAP`** setelah `FE-00` | Backend-nya sudah tersedia (`BE-11`). **Daftar keperluan akses masih kosong** sampai master `BE-09` terisi |
| F2 | `FE-03`, `FE-04` | **`SIAP`** setelah `FE-00` | Backend-nya sudah tersedia (`BE-04`, `BE-06`) |
| F3 | `FE-05` | **`SIAP`** setelah `FE-00` | Backend-nya sudah tersedia (`BE-12`) |
| F3 | `FE-06` | **`SIAP`** setelah `FE-00` | Layar master keperluan akses. Strukturnya siap; isinya menunggu SOP |
| F4 | `FE-07`, `FE-08` | **`SIAP`** setelah `FE-00` | — |
| F5 | `FE-09` | **`SIAP`** setelah seluruh task pendahulu | — |

**Denominator: 10 task. Nol tertahan kontrak. Yang menahan tinggal urutan dependency, dan
`FE-00` adalah pintunya.**

---

## 3. Milestone F0 — Fondasi

### `FE-00` — Lapisan service dan hook

| Field | Isi |
|---|---|
| **Task ID** | `FE-00` |
| **Status** | **`SIAP`** — gerbang kontrak terbuka 27 Agustus 2026 (`RM-DEC-028`). Yang menahan tinggal dependency antar-task pada baris di bawah |
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
| **Status** | **`SIAP`** — gerbang kontrak terbuka 27 Agustus 2026 (`RM-DEC-028`). Yang menahan tinggal dependency antar-task pada baris di bawah |
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
| **Status** | **`SIAP`** — gerbang kontrak terbuka 27 Agustus 2026 (`RM-DEC-028`). Yang menahan tinggal dependency antar-task pada baris di bawah |
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
| **Status** | **`SIAP`** — gerbang kontrak terbuka 27 Agustus 2026 (`RM-DEC-028`). Yang menahan tinggal dependency antar-task pada baris di bawah |
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
| **Status** | **`SIAP`** — gerbang kontrak terbuka 27 Agustus 2026 (`RM-DEC-028`). Yang menahan tinggal dependency antar-task pada baris di bawah |
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
| **Status** | **`SIAP`** — gerbang kontrak terbuka 27 Agustus 2026 (`RM-DEC-028`). Yang menahan tinggal dependency antar-task pada baris di bawah |
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
| **Status** | **`SIAP`** — gerbang kontrak terbuka 27 Agustus 2026 (`RM-DEC-028`). Yang menahan tinggal dependency antar-task pada baris di bawah |
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
| **Status** | **`SIAP`** — gerbang kontrak terbuka 27 Agustus 2026 (`RM-DEC-028`). Yang menahan tinggal dependency antar-task pada baris di bawah |
| **Outcome** | Petugas dapat mencapai layar rekam medis dari menu |
| **Trace** | `RM-CAP-008`; `RM-FE-004`, `RM-FE-013`, `RM-FE-016`; arsitektur frontend bagian 10 |
| **Reuse** | `src/utils/menu-sidebar/menu-items.jsx`. Kedalaman tiga tingkat sudah didukung — `subItems` sudah terdaftar pada `NESTED_MENU_KEYS`. **Tidak ada perubahan komponen sidebar yang diperlukan** |
| **Scope** | Penambahan entri menu sesuai arsitektur frontend bagian 10.2: satu menu induk `Rekam Medis` berisi tiga entri, ditambah satu entri pada `healthServicesMasterData` yang sudah ada |
| **Dependency** | `FE-01` |
| **Acceptance criteria** | 1) Empat entri menu pada arsitektur frontend bagian 10.3 dapat dicapai. 2) **Dihapus pada revisi 2** — "menu hanya muncul bagi peran yang berhak" tidak dapat dibuktikan pada rilis pertama; penyaringan menu per izin belum ada di frontend dan penegakannya ditunda (arsitektur frontend bagian 10.5). Isi tetap ditahan `403`. **Menambal dengan pencocokan nama peran dilarang.** 3) Seluruh berkas `page.jsx` baru lulus uji asap rute |
| **Verification** | `tests/e2e/route-smoke.spec.mjs` |
| **Risk/blocker** | Dua catatan. Pertama: kunci `menuLaboratorium` dan sejenisnya pada `left-sidebar-menu-handle.jsx:3-34` **bukan** definisi menu, melainkan daftar nama kelompok menu bersarang. Menu sesungguhnya ada di `menu-items.jsx`. Kedua: **tidak ada langkah pendaftaran rute** — `route-smoke.spec.mjs:11-54` menelusuri `src/app` sendiri. Membuat berkas halamannya sudah cukup |
| **DoD** | Menu dan rute berjalan; uji asap rute lulus. `RM-FE-013` tercatat sebagai utang terbuka, bukan butir selesai |

### `FE-08` — Penanganan keadaan tidak biasa

| Field | Isi |
|---|---|
| **Task ID** | `FE-08` |
| **Status** | **`SIAP`** — gerbang kontrak terbuka 27 Agustus 2026 (`RM-DEC-028`). Yang menahan tinggal dependency antar-task pada baris di bawah |
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
| **Status** | **`SIAP`** — gerbang kontrak terbuka 27 Agustus 2026 (`RM-DEC-028`). Yang menahan tinggal dependency antar-task pada baris di bawah |
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

**Berubah pada revisi 2.** Brief UI sudah ada — arsitektur frontend bagian 10 dan 11, disahkan
pemilik frontend 27 Agustus 2026. Ruang bebas menyusut dari empat butir menjadi tiga:

| Butir | Ruang bebas yang tersisa |
|---|---|
| `RM-FE-005` | Warna, jarak, tipografi, ikon, dan pemilihan komponen tabel. **Penempatan wilayah layar sudah ditetapkan** pada arsitektur frontend bagian 11 |
| `RM-FE-011` | Cara memuat bertahap: gulir tanpa batas, tombol muat lagi, atau halaman |
| `RM-FE-012` | Bentuk penanda dokumen gagal dimuat sebagian |

Tiga belas butir berikut **tidak** boleh diputuskan developer:

| Butir | Yang mengikat | Asal |
|---|---|---|
| `RM-FE-001` | Penanda status keutuhan wajib terlihat | Keputusan klinis |
| `RM-FE-002` | Addendum wajib menempel pada dokumen induknya | Keputusan klinis |
| `RM-FE-003` | Isian keperluan wajib mendahului tampilnya isi, termasuk mendahului permintaan ke server | Keputusan privasi |
| `RM-FE-006` | Keterangan bahwa label kerahasiaan belum membatasi akses | Keputusan privasi |
| `RM-FE-007` | `PrivateNote` tidak tampil pada tampilan rutin | Keputusan privasi |
| `RM-FE-008` | Dua status harus dapat dibedakan pembaca | Keputusan klinis |
| `RM-FE-009` | Keterangan bahwa baru CPPT yang tunduk aturan keutuhan | Invariant cakupan |
| `RM-FE-010` | Tombol yang tidak berhak ditekan tidak ditampilkan | Invariant |
| `RM-FE-004` | Susunan menu dan rute sesuai arsitektur frontend bagian 10.2 dan 10.3 | Brief UI, revisi 2 |
| `RM-FE-013` | Menu `Tinjauan Akses` hanya bagi pemegang `MedicalRecordAccessLog : Read`. **Penegakannya ditunda**; menambal dengan pencocokan nama peran dilarang | Brief UI, revisi 2 |
| `RM-FE-014` | Berkas dicapai lewat pencarian pasien, bukan daftar seluruh pasien | Brief UI, revisi 2 |
| `RM-FE-015` | Isi dokumen, addendum, dan `PrivateNote` dibuka dari panel detail | Brief UI, revisi 2 |
| `RM-FE-016` | Master keperluan akses di bawah menu `Master Data` yang sudah ada | Brief UI, revisi 2 |

Invariant yang berlaku pada seluruh ruang `DEV_DISCRETION`: apa pun bentuk visual yang dipilih,
**tidak boleh** menyembunyikan keberadaan penanda status, penanda gagal sebagian, maupun
keterangan cakupan. Kebebasan berlaku pada bentuknya, bukan pada ada atau tidaknya.

---

## 10. Yang sengaja tidak masuk roadmap ini

| Yang tidak dikerjakan | Alasan |
|---|---|
| Layar kelengkapan berkas, koding, resume medis, peminjaman | Cakupan 4 sampai 8, rilis berikutnya |
| Layar penetapan penulis berhalangan | **Ditunda 27 Agustus 2026** oleh pemilik frontend. Ketiga endpointnya masih `Rencana`, sehingga tidak ada yang tertahan. Akibatnya `UnitHeadGrant` hanya dapat dibuat lewat API langsung; `InactiveAccount` tidak terpengaruh. Arsitektur frontend bagian 10.6 |
| Penyaringan menu per izin | **Ditunda 27 Agustus 2026.** Mekanismenya belum ada di frontend dan perbaikannya menyentuh seluruh aplikasi, sehingga menjadi task tersendiri di luar roadmap modul ini. Arsitektur frontend bagian 10.5 |
| Portal pasien untuk melihat rekam medisnya sendiri | Di luar scope. `SelfServices` tidak memuat kemampuan klinis apa pun |
| Penyuntingan catatan klinis dari layar rekam medis | Melanggar `RM-DEC-001` |
| Pengunduhan seluruh riwayat sebagai satu berkas | Menciptakan salinan rekam medis di luar sistem tanpa jejak. Perlu keputusan pelepasan informasi lebih dulu |
