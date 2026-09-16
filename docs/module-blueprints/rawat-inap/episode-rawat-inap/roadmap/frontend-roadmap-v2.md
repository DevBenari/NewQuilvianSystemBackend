# Roadmap Delivery Frontend V2 — Sub-modul Episode Rawat Inap

> ## Berkas ini **baru**, dan **tidak menggantikan** `frontend-roadmap.md`
>
> | Hal | `frontend-roadmap.md` (lama) | **`frontend-roadmap-v2.md` (berkas ini)** |
> | --- | --- | --- |
> | Isinya | 28 task revision `4` s.d. `6`, sebagian besar sudah `✅` | **Hanya** task penyelarasan `PRD-RWI-V2-001` revision `7` |
> | Rentang task ID | `FE-RWI-001` s.d. `FE-RWI-062` | **`FE-RWI-063` s.d. `FE-RWI-066`** |
> | Statusnya | **Tetap berlaku** sebagai register task lama | Register task baru |
> | Kenapa dipisah | Permintaan pemilik 16 September 2026: berkas lama sudah terlalu panjang untuk dibaca | — |
>
> **Nomor task tidak pernah dipakai ulang.** Deret `FE-RWI-###` berjalan lurus melintasi kedua
> berkas dan melintasi ketiga sub-modul.
>
> Berkas lama: [`frontend-roadmap.md`](./frontend-roadmap.md) — jangan menambah task baru di sana.

## Metadata

```yaml
module_id: rawat-inap
module_name: InPatientManagement
blueprint_id: RWI-BP-001
blueprint_revision: 7
blueprint_shape: COMPOSITE
submodule: episode-rawat-inap
blueprint_root: docs/module-blueprints/rawat-inap/episode-rawat-inap/
roadmap_file: roadmap/frontend-roadmap-v2.md
roadmap_revision: 1
status: APPROVED
roadmap_mode: DELIVERY
realignment_phase: RLN-PH-07
approval_gate: BLUEPRINT_APPROVED
approved_by: "Muhammad Hamzah — Product/Domain owner (RWI-DEC-061)"
approved_at: "2026-09-16"
approval_decision: RWI-DEC-150
gate_closure_decision: RWI-DEC-151   # {GATE-YOGA} tertutup 2026-09-16
contract_version: 0.9.0
frontend_repo: QuilvianSystemFrontendDev
frontend_branch: HamzahV2
frontend_source_sha: 1ce219b40f8e411f3c4e66975626ab33ae81616a
backend_source_sha: df3679c0d5b2f08106702153eb242d3a6cb2929b
task_id_range: FE-RWI-063..FE-RWI-066
task_id_next_free: FE-RWI-095
stack: "Next.js App Router, JavaScript/JSX, Redux, Axios, design token dan base component Quilvian"
test_policy: "rules/frontend/test-policy.md — menulis test baru opsional; lint dan build wajib"
write_authority: "TIDAK diberikan di sini. Wewenang tulis frontend dinyatakan terpisah per task"
```

## Legenda tanda status

| Tanda | Arti |
| :---: | --- |
| ✅ | Acceptance criteria dan Definition of Done sudah terbukti, laporan tracked-nya ada |
| 🟡 | Source-nya sudah ada tetapi kriterianya belum terbukti penuh |
| ⛔ | Prasyaratnya belum terpenuhi; nama blocker-nya disebut |
| tanpa tanda | Belum disentuh sama sekali |

---

## Grafik Urutan Dependency

```text
BE-RWI-080 [BE] ─> FE-RWI-063

BE-RWI-085 [BE] ─┬─> FE-RWI-064
                 │
BE-RWI-086 [BE] ─┘

BE-RWI-084 [BE] ─> FE-RWI-065

BE-RWI-083 [BE] ─> FE-RWI-066
```

`[BE]` = task backend pada [`backend-roadmap-v2.md`](./backend-roadmap-v2.md), cermin baca-saja.

Keempat task frontend pada roadmap ini **tidak saling menunggu**. Yang ditunggu seluruhnya ada di
backend. Karena itu grafiknya berupa empat rantai terpisah, bukan satu pohon.

Jumlah pasangan prasyarat→task: **5**. Jumlah entri kolom `Dependency`: **5**. Cocok.

### Tabel gelombang eksekusi

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | `BE-RWI-080` [BE] mendarat | `FE-RWI-063` |
| 1 | `BE-RWI-084` [BE] mendarat | `FE-RWI-065` |
| 1 | `BE-RWI-083` [BE] mendarat | `FE-RWI-066` |
| 1 | `BE-RWI-085` **dan** `BE-RWI-086` [BE] mendarat | `FE-RWI-064` |

Keempatnya boleh dikerjakan paralel sepanjang prasyarat backend-nya sudah `✅`.

---

## Tabel task

| Task ID | Outcome | Requirement/decision | Kontrak | Reuse | Cakupan | Dependency | Acceptance criteria | Verifikasi | Risiko/pemilik | DoD |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| ✅ `FE-RWI-063` | Kepala ruangan menambah dan mengakhiri dokter pendukung dari layar | `FR-RI-193`, `194`, `195`; `RWI-DEC-099`, `130` | `0.9.0` API 10.2 | Panel Penugasan Dokter pada `FE-INP-04` | `FE-INP-21` — dialog Dokter Pendukung; tombol disembunyikan bagi selain kepala ruangan dan supervisor | `BE-RWI-080` [BE] | AC-1 s.d. AC-6 | `npm run lint`, `npm run build`, verifikasi manual kontrol interaktif | Tombol tersembunyi bukan pengganti penjagaan server / Muhammad Hamzah | Kartu `FE-RWI-063` |
| `FE-RWI-064` | DPJP mengisi resume delapan bagian dan memakai usulan bersumber | `FR-RI-196`, `FR-RI-197`; `RWI-DEC-112` | `0.9.0` data 18.2–18.3, API 10.3 | Formulir Resume pada `FE-INP-06` | `FE-INP-22` — tiga isian baru; tombol "Isi dari data klinis"; label sumber per bagian | `BE-RWI-085` [BE], `BE-RWI-086` [BE] | AC-1 s.d. AC-6 | `npm run lint`, `npm run build`, verifikasi manual | Usulan tidak boleh tersimpan tanpa tindakan dokter / Muhammad Hamzah | Kartu `FE-RWI-064` |
| `FE-RWI-065` | Petugas melihat peringatan sebelum menutup dan ringkasan akibat sesudahnya | `FR-RI-198`, `199`, `200`, `201`; `RWI-DEC-138`, `143` | `0.9.0` validation matrix | Layar penutupan `FE-INP-07` | `FE-INP-23` — daftar apa yang akan terkunci atau batal, lalu hasilnya | `BE-RWI-084` [BE] | AC-1 s.d. AC-5 | `npm run lint`, `npm run build`, verifikasi manual | Peringatan tidak boleh tampil seperti penghalang / Muhammad Hamzah | Kartu `FE-RWI-065` |
| `FE-RWI-066` | Pesanan tertagih yang tidak dibatalkan punya tempat untuk ditindaklanjuti | `FR-RI-199`; `RWI-DEC-143` (c) | `0.9.0` API 10.4 | Daftar Pantau `FE-INP-09` | `FE-INP-24` — daftar baru pada `FE-INP-09`, urutannya sesuai ketetapan 12 September 2026 | `BE-RWI-083` [BE] | AC-1 s.d. AC-4 | `npm run lint`, `npm run build`, verifikasi manual | Tindak lanjutnya belum diputuskan — 22.7 nomor 1 / Muhammad Hamzah + pemilik Billing | Kartu `FE-RWI-066` |

---

## Kartu task

### ✅ `FE-RWI-063` — `FE-INP-21` Dokter Pendukung

| Field | Isi |
| --- | --- |
| **Status** | ✅ **SELESAI 16 September 2026.** Keenam acceptance criteria terbukti pada source code dan verifikasi kontrol interaktif. `npm run lint:errors`: 0 error; `npm run build`: kompilasi Turbopack sukses, 317 static pages ter-generate, standalone runtime siap (`exit code: 0`). Bukti: [laporan](../task/report/frontend/FE-RWI-063.md) |
| **Gelombang** | 1 — selesai |
| **Layar** | `FE-INP-21` — dialog pada panel Penugasan Dokter di Detail Episode `FE-INP-04` |
| **Jalan masuk** | Tombol "Tambah Dokter Pendukung" pada panel Penugasan Dokter; tombol "Akhiri" pada baris konsulen dan dokter jaga |
| **Hak akses** | `InpatientEpisode : Update` |

**Bisnis prosesnya.** Kepala ruangan membuka Detail Episode Budi, melihat dr. Ahmad sebagai DPJP,
lalu menekan "Tambah Dokter Pendukung". Dialog meminta tiga hal: siapa dokternya, untuk apa
(konsulen, dokter jaga, atau penulisan catatan terlambat), dan alasannya. Bila yang dipilih
penulisan catatan terlambat, dialog **mewajibkan** waktu selesai — karena jendela itu memang harus
punya ujung.

**Cakupan yang diharapkan.**

- Dialog tambah dokter pendukung beserta ketiga isiannya.
- Isian waktu selesai muncul dan menjadi wajib **hanya** saat tujuan `LateDocumentation`.
- Tombol "Akhiri" pada baris konsulen dan dokter jaga; **tidak ada** tombol itu pada baris DPJP.
- Tombol "Tambah Dokter Pendukung" **disembunyikan** bagi pemakai selain kepala ruangan dan supervisor.

**Acceptance criteria.**

1. Dialog mengirim `doctorId`, `assignmentPurpose`, dan `reason` sesuai kontrak `0.9.0` API 10.2.
2. Memilih tujuan `LateDocumentation` memunculkan isian waktu selesai, dan tombol simpan mati selama isian itu kosong.
3. Baris DPJP tidak punya tombol "Akhiri".
4. Perawat pelaksana tidak melihat tombol "Tambah Dokter Pendukung" — `UAT-48`.
5. Galat `422` dari server ditampilkan apa adanya kepada pemakai, bukan ditelan diam-diam.
6. Daftar penugasan di panel menyegar sendiri setelah dialog berhasil.

**Bukti verifikasi.** `npm run lint` dan `npm run build`, keluarannya ditempel apa adanya;
verifikasi manual kontrol interaktif — buka dialog, ganti tujuan, coba simpan tanpa waktu selesai,
simpan yang sah, lalu akhiri satu penugasan. `AUTOMATED TEST: SKIPPED (opsional)` sah untuk task
ini karena perubahannya komposisi view dan wiring thunk, bukan logika murni.

**Yang bukan tanggung jawab layar ini.** Menyembunyikan tombol **bukan** penjagaan. Penolakan
sesungguhnya ada di server lewat `BE-RWI-080`, dan task ini tidak boleh menggantikannya.

**Definition of Done.** Lint dan build hijau; verifikasi manual tercatat; laporan tracked ada di
`task/report/frontend/FE-RWI-063.md`; roadmap dan `requirement-traceability-v2.md` diperbarui.

---

### `FE-RWI-064` — `FE-INP-22` Resume delapan bagian

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 1, menunggu `BE-RWI-085` dan `BE-RWI-086` [BE] |
| **Layar** | `FE-INP-22` — perubahan formulir Resume `FE-INP-06` |
| **Hak akses** | `InpatientDischarge : Read` / `Update` / `Sign` |

**Bisnis prosesnya.** Formulir resume bertambah tiga bagian: Pemeriksaan Penting, Kondisi Saat
Pulang, dan Edukasi. Di atas ketiganya ada tombol "Isi dari data klinis". Menekannya **tidak**
menyimpan apa pun — ia hanya menuangkan usulan ke dalam kotak, masing-masing bertanda dari mana
usulan itu diambil. Dokter menyuntingnya, lalu menandatangani. Yang tersimpan adalah teks final
dokter.

**Contoh.** dr. Rina menekan tombolnya. Kotak Pemeriksaan Penting terisi tiga baris bertanda
"Sumber: Hasil Laboratorium 14 Sep 2026". Ia menghapus satu baris yang tidak relevan, menambah
catatan sendiri, lalu menandatangani.

**Acceptance criteria.**

1. Tiga bagian baru tampil pada formulir resume dan tersimpan lewat kontrak `0.9.0`.
2. Tombol "Isi dari data klinis" hanya aktif pada resume yang **belum** ditandatangani.
3. Setiap bagian yang terisi usulan menampilkan **label sumbernya**.
4. Menekan tombol usulan tidak mengirim permintaan simpan apa pun.
5. Bagian yang sumbernya gagal dibaca tampil kosong berketerangan, dan bagian lain tetap terisi.
6. Resume lama tanpa ketiga isian tetap terbuka dan tetap dapat ditandatangani.

**Bukti verifikasi.** `npm run lint` dan `npm run build`; verifikasi manual — buka resume baru,
tekan tombol usulan, sunting, tanda tangan; lalu buka resume lama dan pastikan tetap normal.

**Catatan lintas sub-modul.** Tab Resume Medis `FE-DOK-12` milik `dokter-rawat-inap`
(`FE-RWI-074`) memakai **kontrak yang sama**. Kedua layar tidak boleh memakai bentuk payload yang
berbeda; bila salah satunya perlu berubah, perubahannya lewat kontrak, bukan lewat salah satu layar.

**Definition of Done.** Lint dan build hijau; verifikasi manual tercatat; laporan tracked ada;
roadmap dan traceability diperbarui.

---

### `FE-RWI-065` — `FE-INP-23` Peringatan dan akibat penutupan

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 1, menunggu `BE-RWI-084` [BE] |
| **Layar** | `FE-INP-23` — perubahan layar penutupan `FE-INP-07` |
| **Hak akses** | `InpatientDischarge : Read` / `Close` / `CloseOverride` |

**Bisnis prosesnya.** Petugas admisi yang menutup episode bukan orang klinis. Sebelum ia menekan
tombol, layar memberitahu apa yang akan terjadi: berapa konsep terkunci, berapa pesanan batal,
berapa dosis batal. Peringatan itu **tidak menahan** — ia memberi tahu, bukan melarang. Sesudah
penutupan berhasil, layar menampilkan apa yang benar-benar terjadi.

**Contoh.** Sebelum menutup episode Joko, layar menampilkan "1 konsep catatan dokter akan terkunci
sebagai Tidak Ditandatangani · 1 pesanan tindakan akan dibatalkan · 1 dosis obat akan dibatalkan".
Petugas menekan Tutup. Layar berikutnya menampilkan ringkasan yang sama sebagai fakta, bukan lagi
sebagai perkiraan.

**Acceptance criteria.**

1. Panel kesiapan menampilkan ketiga jenis akibat beserta jumlahnya, dari endpoint kesiapan.
2. Tombol Tutup **tetap aktif** walau ada peringatan — peringatan tidak pernah mematikannya.
3. Peringatan tampil sebagai informasi, bukan sebagai galat merah yang terbaca seperti larangan.
4. Setelah penutupan berhasil, ringkasan akibat yang ditampilkan berasal dari **respons penutupan**, bukan dari panel kesiapan sebelumnya.
5. Penutupan yang gagal menampilkan "Penutupan gagal disimpan, coba lagi", dan layar tetap pada episode berstatus `DischargePending` — `UAT-51`.

**Bukti verifikasi.** `npm run lint` dan `npm run build`; verifikasi manual — tutup episode yang
punya konsep, pesanan, dan dosis; lalu tutup episode yang bersih dan pastikan panel kesiapan tampil
kosong tanpa galat.

**Definition of Done.** Lint dan build hijau; verifikasi manual tercatat; laporan tracked ada;
roadmap dan traceability diperbarui.

---

### `FE-RWI-066` — `FE-INP-24` Daftar pantau pesanan tertagih

| Field | Isi |
| --- | --- |
| **Status** | Belum dikerjakan |
| **Gelombang** | 1, menunggu `BE-RWI-083` [BE] |
| **Layar** | `FE-INP-24` — daftar baru pada Daftar Pantau `FE-INP-09` |
| **Hak akses** | `InpatientMonitoring : Read` |

**Bisnis prosesnya.** Pesanan tindakan yang sudah ditagih tidak dibatalkan saat episode ditutup.
Kalau tidak ada yang melihatnya, pesanan itu hilang dari perhatian sambil uangnya sudah masuk.
Daftar ini memunculkannya supaya supervisor, admisi, atau billing dapat menindaklanjuti.

**Cakupan yang diharapkan.**

- Daftar baru pada `FE-INP-09`, dengan kolom pasien, episode, pesanan, waktu tutup, dan status tagihan.
- Urutan daftar pada `FE-INP-09` mengikuti ketetapan 12 September 2026: daftar `episode-rawat-inap` lebih dulu — dan `FE-INP-24` menjadi **daftar terakhir** kelompok episode — lalu daftar `dokter-rawat-inap`, lalu `keperawatan`.

**Acceptance criteria.**

1. Daftar memuat persis pesanan tertagih yang tidak dibatalkan saat penutupan.
2. Letaknya pada `FE-INP-09` sesuai urutan yang ditetapkan, bukan disisipkan sembarang.
3. Daftar kosong menampilkan keadaan kosong yang wajar, bukan galat.
4. Setiap baris membuka episode yang bersangkutan.

**Bukti verifikasi.** `npm run lint` dan `npm run build`; verifikasi manual pada Daftar Pantau.

**Risiko yang terbuka.** Apa yang dilakukan **sesudah** pesanan itu terlihat belum diputuskan —
`04-prd-to-mvp.md` 22.7 nomor 1. Layar ini sengaja berhenti pada menampilkan; menambahkan tombol
tindak lanjut apa pun sebelum keputusan itu turun adalah keputusan produk, dan **bukan** wewenang
task ini.

**Definition of Done.** Lint dan build hijau; verifikasi manual tercatat; laporan tracked ada;
roadmap dan traceability diperbarui.

---

## Pilihan UI yang belum disetujui

Tidak ada butir `DEV_DISCRETION` pada roadmap frontend ini. Keempat layar adalah perubahan pada
layar yang sudah ada, dan bentuknya sudah ditetapkan `03-frontend-architecture.md` bagian 12
beserta `05-skema-tampilan.md`. Bila saat implementasi muncul pilihan tata letak yang belum diatur,
pilihan itu dicatat sebagai `DEV_DISCRETION` pada laporan task dan **tidak** diam-diam dijadikan
keputusan produk.

---

## Kebijakan verifikasi frontend

Mengikuti `rules/frontend/test-policy.md`:

- **Menulis test baru bersifat opsional.** Tidak ada task di atas yang tertahan karena tidak menambah test.
- Validasi minimum tetap `npm run lint` dan `npm run build`, dijalankan sungguhan dan keluarannya ditempel.
- `npm run test:unit` menjalankan suite yang sudah ada dan biayanya kecil; jalankan sesuai perubahan.
- `npm run test:e2e` **tidak** dijalankan kecuali task memintanya dan environment mendukung. Repo ini tidak punya `playwright.config.*` di root.
- Baris `AUTOMATED TEST:` dan baris `MANUAL TEST:` ditulis **terpisah** pada laporan task.

---

## Traceability

Tabel penuh ada di [`requirement-traceability-v2.md`](./requirement-traceability-v2.md).

| FR | Epic | Layar | Task frontend | Prasyarat backend | Bukti acceptance |
| --- | --- | --- | --- | --- | --- |
| `FR-RI-193`, `194`, `195` | `RI-39` | `FE-INP-21` | `FE-RWI-063` | `BE-RWI-080` | Acceptance 18.2, `UAT-46` s.d. `48` |
| `FR-RI-196`, `FR-RI-197` | `RI-40` | `FE-INP-22` | `FE-RWI-064` | `BE-RWI-085`, `BE-RWI-086` | Acceptance 18.3, `UAT-49` |
| `FR-RI-198`, `200`, `201` | `RI-41` | `FE-INP-23` | `FE-RWI-065` | `BE-RWI-084` | Acceptance 18.4, `UAT-50`, `UAT-51` |
| `FR-RI-199` | `RI-41` | `FE-INP-24` | `FE-RWI-066` | `BE-RWI-083` | Acceptance 18.4 |

**Gap keterkaitan requirement ke bukti verifikasi:** nol untuk permukaan frontend sub-modul ini.
`FR-RI-191` dan `FR-RI-192` permukaannya ada di ruang kerja dokter `FE-DOK-09`, dan dicatat pada
roadmap frontend `dokter-rawat-inap`, bukan di sini — sesuai aturan satu task muncul di sub-modul
yang memilikinya.
