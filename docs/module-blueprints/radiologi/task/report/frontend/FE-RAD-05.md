# Laporan Perubahan Frontend — `FE-RAD-05`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RAD-05` |
| Judul | Buat pesanan radiologi |
| Epic | `EPIC RAD-05`, `EPIC RAD-07` |
| Requirement | `FR-RAD-042`, `FR-RAD-064` (penomoran blueprint) |
| Decision | `RAD-DEC-013`; `BR-RAD-006` |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 4, gelombang `MVP-3` |
| Contract version | `RAD-API-001` grup *Rad Order* |
| Acceptance criteria | `AC-42` — pesanan ditandai cito: tersimpan siapa yang menandai dan kapan |
| Test yang diminta roadmap | Tombol Simpan ditekan dua kali hanya menghasilkan satu pesanan |
| **Ketentuan mengikat** | `RAD-ARCH-FE-001` bagian 9 — pencegahan kiriman ganda |
| Dependency | `FE-RAD-01` **selesai**; `BE-RAD-12` **selesai** |
| Task mode | `FRONTEND` — frontend target tulis; backend strict read-only kecuali laporan ini |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Branch frontend | `YogaV2`, upstream `origin/YogaV2` |
| Tanggal | 2026-09-11 |
| Status | **Selesai.** 8 unit test baru lulus; 805 unit test repository lulus, 0 gagal; lint 0 error |

---

## 1. Ketentuan mengikat: pencegahan kiriman ganda

`RAD-ARCH-FE-001` bagian 9 menyebut satu penanganan — "tombol dinonaktifkan selama permintaan
berjalan". **Itu dipasang, dan satu penjaga lagi ditambahkan.** Alasannya konkret:

`actionLoading` berasal dari Redux. Ia baru bernilai benar setelah Redux memperbarui state dan
React merender ulang. Dua klik yang sangat berdekatan — atau satu klik ditambah `Enter` pada
form — masih sempat lolos keduanya sebelum render itu terjadi.

Penjaga kedua adalah sebuah `useRef` yang dinyalakan **sinkron** sebelum permintaan berjalan
dan dimatikan pada `finally`. Dua penjaga itu bersama-sama menutup lubangnya.

Akibat bila tidak: seorang pasien punya dua pesanan identik, dan dua pesanan identik berarti
**dua kali penyinaran** bila keduanya sempat dikerjakan.

Empat unit test mengunci perilakunya — dua klik beruntun, tiga klik beruntun, penjaga dilepas
setelah selesai, dan penjaga tetap dilepas ketika permintaan gagal. Yang terakhir penting
tersendiri: penjaga yang tidak dilepas saat gagal mengunci formulir selamanya, dan dokter
tidak dapat mencoba lagi.

---

## 2. Penanda cito — `RAD-DEC-013`, `BR-RAD-006`

`BR-RAD-006` mewajibkan perubahan Cito meminta konfirmasi. Yang dikerjakan layar:

**Centang tidak mengubah nilainya saat ditekan.** Ia hanya membuka konfirmasi; nilainya baru
berubah setelah dokter menyetujui. Centang yang tidak sengaja tersentuh karena itu tidak pernah
tersimpan diam-diam.

**Kalimat konfirmasinya menyebut akibatnya**, bukan sekadar bertanya "yakin?": pesanan cito
mendahului pasien lain yang sudah menunggu lebih lama pada alat yang sama, dan nama penandanya
ikut tercatat. Konfirmasi yang hanya bertanya tidak membuat siapa pun memilih lebih sadar.

Pencabutan penanda juga dikonfirmasi, dengan kalimat dan warna yang berbeda.

`AC-42` — tersimpan siapa yang menandai dan kapan — dipenuhi backend: `RadOrder.CreateAsync`
menstempel `UrgentMarkedByUserId` dan `UrgentMarkedAt` **hanya** ketika penandanya benar-benar
dinyalakan. Layar mengirim `isUrgent` apa adanya.

---

## 3. Satu keputusan yang mencegah pesanan salah jenis

Registry `useSelectResource` untuk `procedures` tidak mendeklarasikan `filterKeys`, dan
`cleanParams` meneruskan seluruh filter ketika daftar izin tidak ada. Jadi
`{ isRadiology: true }` sampai ke `GET /master-data/procedures/options` **tanpa menyentuh
registry bersama**.

Tanpa penyaring itu, formulir radiologi akan menawarkan seluruh tindakan rumah sakit —
termasuk pemeriksaan laboratorium dan tindakan bedah — dan pesanan radiologi dapat dibuat untuk
tindakan yang bukan pencitraan sama sekali. Backend tidak menolaknya: `CreateRadOrderRequest`
hanya menuntut `ProcedureId` yang sah, bukan yang bertanda radiologi.

---

## 4. Peringatan kesiapan alat pada formulir

`RadModalityOptionResponse` membawa `HasActiveSafetyRule`, dan komentar DTO-nya menyatakan
alasannya: "supaya layar yang memilih alat dapat memperingatkan lebih dulu bahwa alat itu akan
menolak pemeriksaan — bukan membiarkan petugas tahu belakangan".

Layar memakainya. Memilih alat yang belum punya aturan berlaku memunculkan peringatan kuning
seketika. **Pesanan tetap boleh disimpan** — menahannya akan menyembunyikan pekerjaan yang
memang perlu dicatat — tetapi dokter tahu sebelum pasien berjalan ke ruang pemeriksaan.

---

## 5. Perubahan yang dikerjakan

| Berkas | Keadaan |
| --- | --- |
| `src/lib/constants/health-services/radiology-management/rad-order-constants.jsx` | Baru — alamat route layar, salinan teks, penyaring pemeriksaan |
| `src/lib/hooks/health-services/radiology-management/use-rad-order-form.jsx` | Baru — controller formulir, dua penjaga kiriman ganda, konfirmasi cito |
| `src/components/view/health-services/radiology-management/rad-orders/form/rad-order-form-view.jsx` | Baru — formulir |
| `src/style/health-services/radiology-management/rad-orders/rad-order.module.css` | Baru — 13 kelas, design token |
| `src/app/health-services/radiology-management/rad-orders/create/page.jsx` | Baru — route, dibungkus `Suspense` karena memakai `useSearchParams` |
| `tests/unit/rad-order-form-rules.test.mjs` | Baru — 8 test |

**Tidak ada berkas bersama yang disentuh.** `store.jsx` tidak berubah — `radOrder` sudah
terdaftar sejak `FE-RAD-01`; `menu-items.jsx` juga tidak, lihat bagian 7.

---

## 6. Validasi

| Yang dijalankan | Hasil |
| --- | --- |
| `node --test tests/unit/rad-order-form-rules.test.mjs` | **8 lulus, 0 gagal** |
| `node --test tests/unit/` (seluruh repository) | **805 lulus, 0 gagal** |
| `npx eslint` atas seluruh berkas yang disentuh | **0 error, 1 warning** |
| `npm run build` | **Compiled successfully in 35.7s.** Route terdaftar: `/health-services/radiology-management/rad-orders/create` (static) |

Satu warning `react-hooks/set-state-in-effect` pada pemuatan daftar alat — jenis yang sama
dengan pola repository.

**`MANUAL TEST: NOT FEASIBLE`.** Menjalankan formulir ini menuntut kunjungan pasien yang aktif
pada lingkungan pengembangan beserta akun dokter yang memegang `RadOrder : Create`, dan
sesi login tidak tersedia dari sini. Perilaku yang paling menentukan — penjaga kiriman ganda —
ditiru persis pada unit test `S5` sampai `S8`, termasuk jalur gagalnya.

---

## 7. Yang sengaja **tidak** dikerjakan

| Butir | Alasan |
| --- | --- |
| Entri menu sidebar | Formulir ini dibuka **dari konteks klinis** dengan `?encounterId=`, bukan dari menu. Modul Laboratorium pun menaruh menunya pada daftar pesanan, bukan formulirnya. Entri menu menyusul bersama `FE-RAD-06` yang membuat daftar antrian |
| Beberapa pemeriksaan dalam satu pesanan | `CreateRadOrderRequest` menerima tepat satu `procedureId`. Menunggu `RAD-CONF-DEC-01` |
| `inpEpisodeId` | Diterima backend dan boleh kosong. Layar ini dibuka dari konteks kunjungan; penanda perawatan rawat inap diisi jalur pemesanan Rawat Inap yang sudah ada, bukan formulir umum ini |
| Penanda cito setelah pesanan dibuat | `PUT /{id}/urgency` sudah tersedia di service sejak `FE-RAD-01`, tetapi layarnya adalah detail pesanan — milik `FE-RAD-06` |
| Base component baru | Dilarang `AGENTS.md` |

---

## 8. Risiko dan dependency yang masih terbuka

| Hal | Keadaan |
| --- | --- |
| Tombol Kembali memakai `router.back()` | Daftar pesanan belum ada sampai `FE-RAD-06`. Memakai `back()` jujur terhadap keadaan itu — dokter kembali ke layar klinis yang memanggilnya — dan akan ditinjau ulang ketika daftarnya berdiri |
| Tidak ada alat yang punya aturan keselamatan aktif | `DEC-RAD-005` `OPEN`. Akibatnya **setiap** alat yang dipilih akan memunculkan peringatan kesiapan pada formulir ini. Itu benar dan memang dimaksudkan — bukan cacat layar |
| Pesanan yang dibuat belum dapat dilihat dari mana pun | Daftar antrian milik `FE-RAD-06`. Sampai itu ada, pesanan hanya terbaca lewat API |

---

## 9. Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 1 | 2026-09-11 | Laporan dibuat. 6 berkas baru, 0 berkas bersama disentuh. 8 test baru lulus; 805 test repository lulus; lint 0 error. | `draft` |
