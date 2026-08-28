# Roadmap Delivery Frontend — Modul IGD

## Metadata

```yaml
module_id: igd
roadmap_revision: 3
wave: "MVP-0..MVP-5 selesai; FE-IGD-020..021 menyusul BE-IGD-037..038"
status: ACTIVE
generated_at: "2026-08-24"
revision_3_at: "2026-08-26"
revision_3_1_at: "2026-08-27"
owners:
  - "Product/Domain Owner IGD — Rizki Gunawan (IGD-DEC-089)"
  - "Frontend authority untuk area DEV_DISCRETION (IGD-UI-004)"
approved_by:
  - "Rizki Gunawan / 2026-08-26 — IGD-DEC-094"
input_revisions:
  blueprint-manifest.md: 5
  03-frontend-architecture.md: 5
  04-prd-to-mvp.md: 5
contract_versions:
  - "State 0.4.0 — bagian 1, 1.1, 1.2 APPROVED (IGD-DEC-093)"
  - "Validation 0.4.0 — bagian 2 aturan 4-5 APPROVED (IGD-DEC-093)"
  - "API 0.4.0 — draft. TIDAK dipakai: gelombang ini nol perubahan endpoint"
artifact_hashes:
  03-frontend-architecture.md: "2b4339f9587ed1daff8444ccb68cb5415df578d76a2157dd3ec168f9a2a1fd95"
  04-prd-to-mvp.md: "7061525001d9a7e6b311424b8e3a8d85de13e35f59e545a78dcefedd600b79db"
  contracts/state-transition-matrix.md: "a41efd8d9adc87e1cf1eec2a9397b3521fdc0ebf935ccf0a19a5aa975b6c7c75"
  contracts/validation-matrix.md: "0ee98b750a29e01603db894ed3766614fe8989b2eef3573eab7d72cdc1a6b907"
source_commits:
  frontend: "96a9120111f6acc6b7c0f37973ea0c717ba41f17"
supersedes: "roadmap/archive/revision-1/frontend-roadmap.md"
```

---

## 0. Gelombang ini nyaris tidak menyentuh frontend

`EPIC IGD-03` adalah perbaikan perilaku backend. **Nol endpoint berubah, nol bentuk response
berubah, nol layar baru.** Yang berubah bagi petugas hanyalah: beberapa perbuatan yang dulu
diam-diam berhasil kini ditolak `409` beserta alasannya.

Karena itu gelombang ini hanya punya **satu** task frontend, dan sifatnya verifikasi.

### Yang sudah benar dan tidak perlu diubah

Diperiksa pada `96a91201`:

| Yang diperiksa | Hasil |
| --- | --- |
| `emergency-triage-form-view.jsx` baris 149 | Sudah punya `errorBanner` yang menampilkan `saveError` |
| `emergency-management-triage-slice.jsx` baris 279–281 | Jalur simpan sudah `catch` dan meneruskan `normalizeErrorMessage(error, …)` |

Pesan `409` dari backend karena itu **sudah punya jalan tampil**. Task di bawah membuktikannya
benar-benar tampil, bukan membangunnya dari nol.

---

## 1. Task

### `FE-IGD-012` — Penolakan `409` jalur triase tampil dengan pesan backend

| Field | Isi |
| --- | --- |
| **Slice** | `IGD-S01` |
| **Scope** | `emergency-triage-form-view.jsx`, `emergency-management-triage-slice.jsx`, dan tab penilaian ulang pada `emergency-assessment-view` |
| **Perubahan** | Diharapkan **nol atau nyaris nol**. Task ini memverifikasi; perubahan hanya ditulis bila verifikasi gagal |
| **Requirement** | `FR-IGD-013`, `FR-IGD-014` — sisi tampilan |
| **Kontrak** | Validation `0.3.0` bagian 2 aturan 4 dan 5 — hash `0ee98b75…` |
| **Dependency** | **`BE-IGD-019` selesai dan berjalan.** Sebelum itu penolakan yang harus ditampilkan belum ada |
| **Acceptance** | 1. Menyelesaikan triase pada kunjungan yang sudah ditutup menampilkan pesan backend apa adanya — *"Kunjungan IGD sudah ditutup, penilaian tidak dapat diselesaikan."* — **bukan** pesan cadangan *"Gagal menyimpan pemeriksaan triage."* 2. Penolakan transisi menampilkan pesan yang menyebut status kunjungan saat ini. 3. Tidak ada layar yang menampilkan status kunjungan sebagai `Triaged` setelah pasien `InTreatment` dinilai ulang. 4. Bila `normalizeErrorMessage` ternyata tidak membaca `response.data.message` untuk `409`, perbaiki **di tempat yang sudah ada**, jangan membuat penangan error tandingan |
| **Test** | `AT-IGD-086` sisi tampilan; `npm run lint` dan `npm test` tetap lulus |
| **Bukti** | Tangkapan layar kedua penolakan; catatan hasil untuk keempat butir acceptance |
| **Risiko** | Rendah |
| **Kewenangan UI** | **Tidak ada layar baru, tidak ada komponen baru, tidak ada CSS baru.** Bila butir 4 menuntut perubahan, ikuti `errorBanner` dan pola gaya yang sudah dipakai layar triase |
| **Owner** | Frontend |

---

## 2. Warisan revision `1` yang belum selesai

Task berikut **bukan** bagian `MVP-0`, tetapi belum dikerjakan dan tidak boleh hilang karena
pergantian revisi roadmap.

| Task | Isi | Keadaan | Catatan |
| --- | --- | --- | --- |
| `FE-IGD-010` | Halaman detail satu kunjungan IGD | **Belum dikerjakan.** Seluruh dependency-nya (`FE-IGD-004`, `007`, `008`, `009`) sudah selesai | Rincian penuh ada di `roadmap/archive/revision-1/frontend-roadmap.md` bagian `FE-IGD-010` |

`FE-IGD-010` dapat dikerjakan kapan saja dan **tidak** bergantung pada gelombang ini. Ia juga
tidak terpengaruh `IGD-DEC-091`: penggantian nama `emergency-transfers` menjadi
`emergency-departures` baru berlaku pada `MVP-3`, dan `FE-IGD-010` menampilkan data, bukan
memanggil route perpindahan.

`FE-IGD-001` sampai `FE-IGD-009` dan `FE-IGD-011` sudah selesai pada revision `1`.

---

## 3. Yang menunggu gelombang berikutnya

> **Digantikan revision `3`.** Tabel ini benar saat revision `2` ditulis. Revision `3`
> menomori ulang gelombang — kepergian pasien pindah dari `MVP-3` ke `MVP-4`, dan pengkajian
> IGD naik dari `POST-MVP` ke `MVP-3` atas dasar bukti baru. Yang berlaku adalah bagian R3.
> Tabel ini disimpan sebagai catatan keadaan saat itu.

| Pekerjaan frontend | Menunggu | Sebabnya |
| --- | --- | --- |
| Mengganti `TRANSFER_URL` menjadi `emergency-departures` | `MVP-3` | `IGD-DEC-091`. Satu baris pada `emergency-assessment-slice.jsx:16`; **jangan** diubah sebelum backend-nya berganti |
| Layar dua rangkaian status kepergian | `MVP-3` | `IGD-DEC-090` |
| Layar koreksi dan pembalikan berpersetujuan | `MVP-3` | `IGD-DEC-090` |
| Serah terima SBAR dan sikap pesanan | `MVP-4` | `EPIC IGD-07` |
| Penanda unit tanpa kewenangan | `MVP-5` | `IGD-DEC-092`, dan pengesahan Security/Privacy owner |
| Layar pengkajian IGD tersimpan sungguhan | `POST-MVP` | Pemilik `ClinicalManagement` belum ditunjuk |

---

## 4. Definition of Done gelombang `MVP-0` — sisi frontend

| No | Butir | Bukti yang diterima |
| ---: | --- | --- |
| 1 | `FE-IGD-012` keempat butir acceptance-nya terjawab | Catatan hasil beserta tangkapan layar |
| 2 | `npm run lint` lulus | Keluaran perintah |
| 3 | `npm test` lulus | Keluaran perintah |
| 4 | Nol komponen, layar, atau modul CSS baru | Diff; bila kosong, sebutkan kosong |
| 5 | Alur simpan dijalankan sungguhan lewat layar | **Belum pernah terpenuhi** — butuh kredensial petugas. Bila masih belum ada, catat sebagai belum terbukti, jangan ditandai lulus |

Butir 5 adalah utang lama yang berlaku sejak revision `1` dan **tidak** diselesaikan gelombang
ini. Ia dicatat supaya tidak hilang, bukan supaya dianggap selesai.

---

# Revision 3 — perluasan ke perjalanan pasien penuh

Ditambahkan 26 Agustus 2026. Revision `2` di atas tetap berlaku.

## R3.0 Temuan yang mengubah bentuk pekerjaan frontend

Diperiksa pada `96a91201`.

**Layar pengkajian IGD sudah dibangun.** `emergency-assessment-view` memuat sebelas komponen:
SOAP, tanda vital, observasi, disposisi, transfer, catatan terintegrasi, nosokomial, rekam,
kartu pasien, kartu formulir, dan section. Ditambah `emergency-assessment-list-view` dan
`emergency-assessment-detail-view`.

Artinya pekerjaan frontend untuk pengkajian **bukan membangun layar**, melainkan membuktikan
bahwa layar yang sudah ada benar-benar menyimpan setelah backend dibuka. Utang lama
*"alur simpan lewat layar belum pernah dijalankan sungguhan"* akhirnya dapat dilunasi —
tetapi hanya setelah `BE-IGD-027`.

| Yang diperiksa | Hasil |
| --- | --- |
| `emergency-assessment-view` | **11 komponen tab**, lengkap |
| `emergency-triage` dan `emergency-registration` | Ada, punya halaman `page.jsx` |
| `app/…/emergency-pengkajian/` | **Folder kosong, nol berkas** — rute yang tidak pernah jadi |
| `components/view/…/emergency-installation-management/test/test.txt` | Berkas sisa yang tidak dipakai |

## R3.1 Task

### `FE-IGD-013` — Pengkajian IGD benar-benar tersimpan lewat layar

| Field | Isi |
| --- | --- |
| **Slice** | `IGD-S04` · `EPIC IGD-09` |
| **Scope** | `emergency-assessment-view` beserta sebelas tab-nya |
| **Perubahan** | Diharapkan **kecil**. Layarnya sudah ada; yang dikerjakan adalah menyambungkan ke jalur simpan yang baru terbuka, dan memperbaiki hanya bagian yang terbukti gagal |
| **Requirement** | `FR-IGD-060` … `FR-IGD-064` sisi tampilan |
| **Dependency** | **`BE-IGD-027` dan `BE-IGD-028` selesai dan berjalan.** Sebelum itu tidak ada yang dapat dibuktikan |
| **Acceptance** | 1. Perawat mengisi pengkajian pasien IGD, menekan simpan, dan datanya **benar-benar ada** saat layar dibuka ulang. 2. Setiap field yang tampil di layar punya rumah di basis data — pelajaran `review-triage-igd-alur-penyimpanan` yang menemukan 17 dari 20 field hilang diam-diam. 3. Field yang belum punya rumah **ditandai jelas di laporan**, bukan dibiarkan tampil seolah tersimpan. 4. Tanda vital, diagnosis, tindakan, dan CPPT ikut dibuktikan |
| **Bukti** | Tangkapan layar sebelum dan sesudah muat ulang, plus kueri jumlah baris per tabel |
| **Risiko** | **Menengah.** Butuh kredensial petugas — utang yang belum pernah terlunasi sejak revision `1` |
| **Kewenangan UI** | Nol layar baru. Ikuti `emergency-triage.module.css` dan komponen `form-pemeriksaan-ui` yang sudah dipakai |
| **Owner** | Frontend |

### `FE-IGD-014` — Pendaftaran IGD mengikuti `EncounterType.Emergency`

| Field | Isi |
| --- | --- |
| **Slice** | `IGD-S02`, `IGD-S03` · `EPIC IGD-01`, `EPIC IGD-02` |
| **Scope** | `registration-management/emergency-registration` |
| **Perubahan** | Menyesuaikan jenis kunjungan yang dikirim, dan menampilkan penolakan episode ganda beserta **nomor kunjungan yang sudah ada** sebagai jalan pintas yang dapat diklik petugas |
| **Requirement** | `FR-IGD-001` … `FR-IGD-012` sisi tampilan |
| **Dependency** | `BE-IGD-023`, `BE-IGD-025` |
| **Acceptance** | 1. Pendaftaran IGD baru terkirim sebagai `Emergency`. 2. Pendaftaran kedua untuk pasien yang sama menampilkan nomor kunjungan pertama, dan petugas dapat langsung membukanya — bukan sekadar pesan gagal. 3. Jalan keluar beralasan tersedia dan alasannya wajib diisi |
| **Risiko** | Menengah. Ini pintu masuk pasien; salah sedikit, pendaftaran berhenti |
| **Owner** | Frontend |

### `FE-IGD-015` — Route `emergency-departures`

| Field | Isi |
| --- | --- |
| **Slice** | `IGD-S05` · `EPIC IGD-05` |
| **Scope** | `emergency-assessment-slice.jsx` baris 16 — konstanta `TRANSFER_URL`. **Satu baris** |
| **Perubahan** | `emergency-transfers` menjadi `emergency-departures`, dirilis **bersamaan** dengan `BE-IGD-031`. Tidak ada route usang di backend, sehingga mendahului atau terlambat sama-sama memutus |
| **Keputusan** | `IGD-DEC-091` |
| **Dependency** | `BE-IGD-031` — **rilis serentak, bukan berurutan** |
| **Acceptance** | 1. Tab Transfer tetap bekerja. 2. Nol URL halaman berubah — tab ini ada di dalam `emergency-assessment/[slug]`, jadi nol bookmark petugas rusak |
| **Risiko** | Rendah secara teknis; **koordinasi rilisnya** yang berisiko |
| **Owner** | Frontend, serentak dengan Backend |

### `FE-IGD-016` — Dua rangkaian status kepergian

| Field | Isi |
| --- | --- |
| **Slice** | `IGD-S05` · `EPIC IGD-05` |
| **Scope** | `emergency-assessment-transfer-tab.jsx` |
| **Perubahan** | Satu status tunggal menjadi dua yang berdampingan: keadaan fisik pasien dan keadaan serah terima. Keduanya bergerak sendiri-sendiri |
| **Keputusan** | `IGD-DEC-090` |
| **Dependency** | `BE-IGD-032` |
| **Acceptance** | 1. Kedua rangkaian terbaca sekaligus tanpa perlu berpindah tab. 2. Kombinasi yang mustahil tidak dapat dipilih. 3. Petugas dapat membedakan "pasien sudah berangkat" dari "unit tujuan sudah menerima" — dua hal yang selama ini tertukar |
| **Risiko** | Menengah. Dua status berdampingan mudah membingungkan bila penamaannya tidak jelas |
| **Owner** | Frontend |

### `FE-IGD-017` — Entri susulan, koreksi, dan daftar pantau

| Field | Isi |
| --- | --- |
| **Slice** | `IGD-S05` · `EPIC IGD-06` |
| **Scope** | Tab kepergian, ditambah satu daftar pantau |
| **Perubahan** | Mencatat waktu kejadian sebenarnya yang berbeda dari waktu pencatatan; menampilkan riwayat koreksi tanpa menyembunyikan yang lama; pembalikan menampilkan siapa yang menyetujui |
| **Keputusan** | `IGD-DEC-065`, `IGD-DEC-066`, `IGD-DEC-085`, `IGD-DEC-090` |
| **Dependency** | `BE-IGD-033`, `BE-IGD-034` |
| **Acceptance** | 1. Riwayat kejadian terbaca urut beserta pelakunya. 2. Baris yang sudah dikoreksi **tetap terlihat**, ditandai tidak berlaku — tidak dihapus dari layar. 3. Waktu sebenarnya di masa depan ditolak di layar, bukan hanya di backend |
| **Risiko** | Menengah |
| **Owner** | Frontend |

### `FE-IGD-018` — Bersih-bersih sisa yang tidak dipakai

| Field | Isi |
| --- | --- |
| **Slice** | Kebersihan. Bukan bagian epic mana pun |
| **Scope** | `app/health-services/emergency-installation-management/emergency-pengkajian/` yang **kosong**, dan `components/view/…/emergency-installation-management/test/test.txt` |
| **Perubahan** | Menghapus keduanya **setelah dipastikan tidak ada yang menunjuk ke sana**. Folder rute kosong pada App Router membingungkan: ia tampak seperti halaman yang ada padahal tidak |
| **Dependency** | Tidak ada |
| **Acceptance** | 1. Penelusuran menunjukkan nol rujukan ke keduanya. 2. `npm run build` dan `npm run lint` tetap lulus. 3. Nol rute yang sebelumnya bekerja menjadi rusak |
| **Risiko** | Rendah. **Periksa dulu, hapus kemudian** |
| **Owner** | Frontend |

## R3.2 Urutan

```
BE-IGD-023 ──► FE-IGD-014 (pendaftaran)
BE-IGD-025 ──┘

BE-IGD-027 ──► FE-IGD-013 (pengkajian)  ← melunasi utang "belum pernah dijalankan sungguhan"
BE-IGD-028 ──┘

BE-IGD-031 ══► FE-IGD-015 (route)       ← RILIS SERENTAK, bukan berurutan
BE-IGD-032 ──► FE-IGD-016 (dua status)
BE-IGD-033 ──► FE-IGD-017 (koreksi)
BE-IGD-034 ──┘

FE-IGD-018 dan FE-IGD-010 bebas kapan saja
```

## R3.3 Yang belum dapat direncanakan

Penunjang medis, pemakaian alat, dan billing IGD **belum punya blueprint**, sehingga belum
punya task frontend. Lihat `backend-roadmap.md` bagian R3.5.

Satu hal yang sudah pasti sekarang: layar penunjang medis tidak dapat menampilkan hasil
pemeriksaan, karena `LabOrder` **tidak menyimpan hasil sama sekali** — hanya `EncounterId` dan
`ProcedureId`. Layar apa pun yang dibuat sekarang hanya akan menampilkan daftar pesanan
kosong.

---

## R3.4 Gelombang 27 Agustus 2026

Laporan lengkapnya di
`task/report/frontend/fe-igd-020-021-route-master-igd-dan-kolom-kesimpulan.md`.

| Task | Judul | Status |
| --- | --- | --- |
| `FE-IGD-020` | Route master data IGD mengikuti pemindahan modul | **Selesai** |
| `FE-IGD-021` | Kolom "Kesimpulan" pada Riwayat Observasi tidak pernah terisi | **Selesai** |

### `FE-IGD-020` — RILIS SERENTAK

`BE-IGD-037` mengubah route master data IGD:

```
sebelum : /v1/health-services/master-data/emergency-installation-management/<res>
sesudah : /v1/health-services/emergency-installation-management/master-data/<res>
```

Lima pemanggilan di empat berkas disesuaikan; nol rujukan route lama tersisa.

> Ini perubahan kontrak, sama seperti `FE-IGD-015`. Frontend baru terhadap backend lama akan
> `404` pada seluruh pilihan master IGD — cara kedatangan dan jenis kasus pada layar
> pendaftaran, level dan indikator triase, jenis tindak lanjut pada layar pengkajian.
> **Keduanya wajib naik bersamaan.**

### `FE-IGD-021` dan pemeriksaan kolom layar

Tab Observasi merujuk `conclusion`, sedangkan `EmergencyObservationResponse` menamainya
`completionSummary`. Kolomnya kosong selamanya tanpa pernah melempar galat.

Ditemukan lewat pemeriksaan menyeluruh yang membandingkan **setiap kolom yang dideklarasikan
layar pengkajian** dengan isi respons daftar sungguhan. Sembilan bagian diperiksa; tiga cacat
ditemukan — satu diperbaiki di frontend, dua di backend (`BE-IGD-038`). Sesudahnya, bagian yang
punya data menunjukkan **nol kolom hilang**.

**Pelajaran:** kolom yang dideklarasikan layar tetapi tidak dikirim endpoint tidak pernah
tampil sebagai galat, hanya kosong. Membaca kode layar saja tidak cukup, membaca DTO saja juga
tidak — keduanya terlihat wajar sendiri-sendiri.

### Peringatan perkakas: `npm test` dapat lulus tanpa menjalankan test

`npm test` memakai `node --test "tests/unit/**/*.test.mjs"`. Node **v20** belum mendukung glob
pada `--test` — dukungan itu masuk pada Node 21 — sehingga perintahnya gagal menemukan berkas,
**nol test berjalan**, tetapi exit code-nya tetap `0`.

Sampai `package.json` atau versi Node-nya diperbaiki, jalankan
`node --import ./tests/helpers/register.mjs --test tests/unit` supaya angkanya benar-benar
berarti. Per 27 Agt: **119 lulus, 0 gagal**.
