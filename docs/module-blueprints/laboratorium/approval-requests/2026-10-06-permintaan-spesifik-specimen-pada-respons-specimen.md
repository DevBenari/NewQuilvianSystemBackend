# Permintaan Persetujuan — Spesifik Specimen pada Respons Specimen

| Field | Value |
|---|---|
| `request_id` | `LAB-REQ-015` |
| `tanggal` | 2026-10-06 |
| `pengaju` | Implementer frontend Laboratorium, sesudah verifikasi susulan `FE-LAB-32` |
| `rujukan` | `LAB-DEC-098`, `LAB-DEC-107`, `LAB-DEC-112`; `LAB-API-v1` `r26` 21.4; `AC-160`, `AC-170`, `AC-175`; `VAL-105`; [`FE-LAB-32.md`](../task/report/frontend/FE-LAB-32.md) 9.4 |
| `status` | ✅ **`disetujui`** 2026-10-06 — Yoga Aji Pratama, pemilik modul: *"yaaa saya setujui"*; **pilihan A pada ketiga butir** |
| `ditujukan kepada` | Yoga Aji Pratama — pemilik modul Laboratorium |
| `sifat` | Usulan amandemen kontrak beserta satu task backend dan sisa satu task frontend. **Sudah diterapkan** ke artefak kanonis 2026-10-06 — bagian 8 |

---

## 1. Satu paragraf untuk yang tidak punya waktu

Petugas dapat mengoreksi Informasi Specimen dari Halaman Hasil Mikrobiologi, tetapi **formulirnya
selalu mulai kosong**. Penyebabnya di backend: respons specimen tidak pernah menyebut Spesifik Specimen
mana yang sudah tercatat. Akibatnya, mencentang satu kotak **diam-diam mengganti seluruh pilihan** yang
sudah ada. Yang diminta: respons specimen membawa daftar Spesifik Specimen yang tercatat, dan aturan
penggantiannya diperhalus supaya pilihan lama tidak ikut rusak. Aditif pada respons, **nol migration,
nol izin baru, nol endpoint baru**. Inilah satu-satunya penahan yang membuat `FE-LAB-32` belum dapat
naik ke ✅.

---

## 2. Bukti

Diverifikasi di peramban 2026-10-02 dan 2026-10-06 (laporan `FE-LAB-32.md` bagian 9), lalu dibaca pada
source backend 2026-10-06.

| # | Yang ditemukan | Bukti |
|---:|---|---|
| 1 | Formulir koreksi mulai kosong walau specimen sudah berisi — volume `2,5 L/jam` yang baru disimpan tidak tampil saat halaman dimuat ulang | Uji K4 (`FAIL`); `EMPTY_SPECIMEN_FORM` pada `use-lab-microbiology-result-editor.jsx` |
| 2 | Layar **tidak pernah** memuat data specimen: `specimenId` diambil dari daftar pemeriksaan, tanpa satu pun pembacaan specimen | `lab-microbiology-result-panel.jsx` baris 32 |
| 3 | `LabSpecimenResponse` memuat jenis, volume, satuan, keterangan, dan waktu terima — tetapi **nol ruas Spesifik Specimen** | `DTOs/LabSpecimenDtos.cs`, kelas `LabSpecimenResponse` |
| 4 | `detailTypeIds` pada koreksi **menggantikan seluruh** pilihan (`r26` 21.4) — layar yang tidak tahu pilihan lama pasti menghapusnya bila satu kotak dicentang | `LabSpecimenCorrectionService.ApplyDetailsAsync` — `RemoveRange` seluruh baris lalu `Add` yang dikirim |
| 5 | Koreksi **ditolak seluruhnya** bila satu Spesifik Specimen yang dikirim sudah nonaktif — termasuk pilihan lama yang hanya dikirim ulang | Pesan *"Sebagian rincian specimen yang dipilih sudah tidak dipakai lagi."*; tanpa kode `VAL` |
| 6 | Penggantian menulis ulang **nama snapshot** setiap rincian, termasuk yang tidak diubah — nama yang kelak diperbaiki kepala instalasi ikut mengubah arti specimen lama | Baris baru selalu `DetailNameSnapshot = nama data induk saat ini`; bertentangan dengan alasan snapshot pada `LabSpecimenDetail` |
| 7 | **Selisih dokumen:** `r26` 21.4 menulis respons `PATCH /correction` = `ApiResponse<LabSpecimenResponse>`; source mengembalikan `ApiResponse<object>` berisi pesan jumlah ruas | `LabSpecimenController.ApplyCorrection`; pesan *"Informasi Specimen berhasil dikoreksi; 3 ruas tercatat."* |

Butir 1–2 dapat ditutup frontend sendiri **kecuali** Spesifik Specimen (butir 3). Butir 4–6 hanya dapat
ditutup backend.

---

## 3. Keputusan yang diminta — usulan `LAB-DEC-167`

Tiga butir. Masing-masing berisi rekomendasi; pilihan lain tetap sah.

### 3.1 Bentuk ruas

| Pilihan | Isi | Penilaian |
|---|---|---|
| **A (rekomendasi)** | `specimenDetails`: daftar `{ labSpecimenDetailTypeId, detailName, isActive }`. `detailName` = **nama snapshot** yang tercatat; `isActive` = keadaan data induknya hari ini | Layar dapat mencentang yang tersimpan, menampilkan nama **sebagaimana tercatat**, dan menandai pilihan yang kini nonaktif |
| B | `detailTypeIds`: daftar id saja | Cukup untuk mencentang, tetapi nama yang tampil akan diambil dari data induk hari ini — bukan nama yang tercatat — dan pilihan nonaktif tidak terlihat sebabnya |

### 3.2 Endpoint yang membawanya

| Pilihan | Isi | Penilaian |
|---|---|---|
| **A (rekomendasi)** | `GET /lab-specimens/by-order/{labOrderId}` — **sudah ada**, sudah dipakai layar pesanan (`getLabSpecimensByOrder`), hak `LabSpecimen : Read` | Nol endpoint baru. Satu pesanan Mikrobiologi biasanya memuat satu–dua wadah |
| B | `GET /lab-specimens/{id}` baru | Satu wadah persis, tetapi menambah endpoint dan pola baca baru untuk kebutuhan yang dipenuhi A |

Respons tindakan siklus hidup (`collect`, `receive`, `accept`, …) dan daftar berhalaman `GET /` mengirim
`specimenDetails` = **`null`** — artinya *tidak dimuat*, bukan *nol rincian* — mengikuti pola `r38` 33.2.

### 3.3 Aturan penggantian pada `PATCH /correction`

| Pilihan | Isi | Penilaian |
|---|---|---|
| **A (rekomendasi)** | Penggantian **berdasar selisih**: rincian yang sudah tercatat dan dikirim ulang **dibiarkan** — barisnya, nama snapshot-nya, dan keadaannya walau data induknya kini nonaktif; yang tidak dikirim dihapus; yang baru ditambahkan dan **wajib aktif**. Menambah rincian nonaktif ditolak `422` dengan kode baru `VAL-150` | Menutup butir 5 dan 6 sekaligus. Petugas dapat mengoreksi volume tanpa dipaksa melepas pilihan lama yang sah ketika dicatat |
| B | Tetap seperti sekarang — semua yang dikirim wajib aktif | Layar wajib memaksa petugas **melepas** pilihan yang kini nonaktif sebelum boleh menyimpan apa pun — mengubah catatan bahan hanya karena data induknya dirapikan |

Jejak ruas (`LAB-DEC-112`) tetap **satu baris** berisi daftar nama lama dan baru, dan **nol baris** bila
pilihannya tidak berubah — perilaku pencatat hari ini (`LabFieldChangeRecorder` melewati nilai yang sama).

---

## 4. Usulan amandemen kontrak — `LAB-API-v1` `r39` dan `LAB-VAL-v1` `r16`

Teks di bawah siap ditempel sebagai bagian 34 `api-contract.md` bila pilihan A disetujui pada ketiga
butir.

> ### 34. Amandemen `r39` — Spesifik Specimen pada respons specimen, 2026-10-06
>
> | Field | Nilai |
> |---|---|
> | Status | **`usulan`** |
> | Keputusan | `LAB-DEC-167` |
> | Alasan | Formulir koreksi Informasi Specimen (`FE-LAB-32`) tidak dapat menampilkan Spesifik Specimen yang tercatat, padahal `detailTypeIds` menggantikan seluruh pilihan (21.4) |
> | Sifat | **Aditif** pada respons; **satu perubahan perilaku** pada penggantian (34.3); nol endpoint baru, nol migration, nol izin baru |
> | Task | `BE-LAB-88` (backend), sisa `FE-LAB-32` (layar) |
>
> #### 34.1 Endpoint
>
> | Method | Path | Perubahan | Hak akses |
> |---|---|---|---|
> | `GET` | `/lab-specimens/by-order/{labOrderId}` | Setiap wadah membawa `specimenDetails` | `LabSpecimen : Read` — tidak berubah |
> | `PATCH` | `/lab-specimens/{id}/correction` | Aturan penggantian `detailTypeIds` berdasar selisih (34.3). **Respons dikoreksi pada dokumen:** `ApiResponse<object>` berisi pesan jumlah ruas tercatat — sesuai source sejak `BE-LAB-57`, bukan `LabSpecimenResponse` seperti tertulis di 21.4 | `LabSpecimen : Update` — tidak berubah |
>
> #### 34.2 Ruas yang bertambah pada `LabSpecimenResponse`
>
> | Ruas | Tipe | Boleh kosong | Arti |
> |---|---|:---:|---|
> | `specimenDetails` | `LabSpecimenDetailItem[]` | Ya | Spesifik Specimen yang tercatat, urut nama. **`null` = tidak dimuat** (respons tindakan siklus hidup dan `GET /`); **`[]` = nol rincian** |
>
> **`LabSpecimenDetailItem`**
>
> | Ruas | Tipe | Arti |
> |---|---|---|
> | `labSpecimenDetailTypeId` | `Guid` | Data induk yang dipilih |
> | `detailName` | `string` | **Nama snapshot** saat dipilih (`LabSpecimenDetail.DetailNameSnapshot`) — bukan nama data induk hari ini |
> | `isActive` | `bool` | Keadaan data induknya **hari ini**; `false` berarti kepala instalasi sudah menonaktifkannya |
>
> #### 34.3 Penggantian berdasar selisih
>
> Bila `detailTypeIds` dikirim: id yang **sudah tercatat** pada wadah itu dibiarkan apa adanya (baris,
> nama snapshot, dan keadaan nonaktifnya); id tercatat yang **tidak dikirim** dihapus; id **baru** wajib
> menunjuk data induk aktif (`VAL-150`). `VAL-105` (rincian ganda) tetap. Jejak ruas satu baris, dan nol
> baris bila himpunannya sama.

Dan satu baris baru `validation-matrix.md` (`LAB-VAL-v1` `r16`):

| ID | Aturan | Pesan | Sumber |
|---|---|---|---|
| `VAL-150` | Spesifik Specimen yang **baru ditambahkan** pada koreksi wajib menunjuk data induk aktif; yang sudah tercatat boleh dikirim ulang walau kini nonaktif | "Rincian specimen ini sudah tidak dipakai lagi dan tidak dapat ditambahkan." | `LAB-DEC-167` butir 3 |

---

## 5. Usulan task backend — `BE-LAB-88`

| Butir | Isi |
|---|---|
| **Status** | `USULAN` — menunggu `LAB-DEC-167` dan `r39` |
| **Outcome** | Respons specimen menyebut Spesifik Specimen yang tercatat; koreksi tidak lagi merusak pilihan lama |
| **Requirement/decision** | `LAB-DEC-167` (usulan); `LAB-DEC-098`, `LAB-DEC-107`, `LAB-DEC-112` |
| **Kontrak** | `r39` 34.1–34.3; `LAB-VAL-v1` `r16` `VAL-150` |
| **Reuse** | Tabel `LabSpecimenDetail` dan navigasi `LabSpecimenDetailType` yang sudah ada; pencatat jejak `LabFieldChangeRecorder` |
| **Cakupan** | (1) `DTOs/LabSpecimenDtos.cs`: kelas `LabSpecimenDetailItem` dan ruas `SpecimenDetails` (`List<LabSpecimenDetailItem>?`) pada `LabSpecimenResponse`. (2) `LabSpecimenService.GetByOrderAsync`: proyeksi sub-koleksi `LabSpecimenDetails` per wadah (`!IsDelete`), urut `DetailNameSnapshot`, `IsActive` dari `LabSpecimenDetailType`. (3) `LabSpecimenCorrectionService.ApplyDetailsAsync`: penggantian berdasar selisih dan `VAL-150`. (4) `MapResponse` (respons siklus hidup) dan daftar berhalaman **tidak** diubah — ruasnya `null` |
| **Dependency** | — |
| **Acceptance criteria** | (a) `by-order` memuat `specimenDetails` sama persis dengan baris `LabSpecimenDetail` wadah itu, nama = snapshot. (b) Wadah tanpa rincian → `[]`. (c) Koreksi volume dengan `detailTypeIds` = himpunan yang sama → nol baris jejak Spesifik Specimen, baris dan snapshot tidak berubah. (d) Rincian tercatat yang kemudian dinonaktifkan **tetap dapat dikirim ulang** — koreksi berhasil. (e) Menambah rincian nonaktif → `422` `VAL-150`. (f) Melepas satu dari dua → satu baris jejak berisi nama lama dan baru. (g) Respons siklus hidup tidak berubah bentuk selain ruas `null` |
| **Verifikasi** | Lewat HTTP pada specimen uji `14e5794d…` (`LAB-RSMMC-000014`, dua Spesifik Specimen tercatat 2026-10-02) dan harness untuk butir (d)–(e) yang menuntut menonaktifkan data induk. Butir (d)–(e) **tidak** dijalankan dengan menonaktifkan data induk dev bersama tanpa izin |
| **Risiko/pemilik** | **Rendah.** Jebakan: memproyeksikan nama dari data induk alih-alih snapshot; dan menyamakan `null` dengan `[]`. Pemilik: implementer backend |
| **DoD** | Ketujuh AC terbukti; build hijau (`-p:RunAnalyzers=False`); laporan `BE-LAB-88.md`; `r39` kolom *Status* diperbarui |

---

## 6. Usulan sisa `FE-LAB-32`

Bukan nomor task baru — **penutup outcome** `FE-LAB-32`, mengikuti preseden `FE-LAB-31-sisa-mvp7b.md`.

| Butir | Isi |
|---|---|
| **Status** | `USULAN` — menunggu `BE-LAB-88` |
| **Cakupan** | (1) Hook editor memuat `getLabSpecimensByOrder(labOrderId)` dan memilih wadah `specimenId`. (2) Formulir koreksi diisi nilai tersimpan: jenis, keterangan *Lainnya*, Spesifik Specimen tercentang, volume (koma desimal), satuan, keterangan, waktu terima fisik. (3) Rincian tercatat yang **nonaktif** tetap tampil tercentang dengan tanda *"sudah tidak dipakai"* dan dapat dilepas, tetapi tidak dapat dicentang ulang sesudah dilepas. (4) Sesudah koreksi berhasil, data specimen dimuat ulang. (5) Payload tetap hanya membawa ruas yang disentuh |
| **Acceptance criteria** | `AC-160`, `AC-162`, `AC-170` tetap; ditambah: formulir terisi nilai tersimpan (uji K4 ulang); mengoreksi volume **tidak** mengubah Spesifik Specimen; melepas satu rincian hanya melepas yang itu |
| **Verifikasi** | Peramban, akun analis asli + superadmin pengganti pemegang `LabSpecimen : Update`, tulis hanya pada specimen uji `14e5794d…`. Pastikan jabatan analis memegang `LabSpecimen : Read` — bila tidak, formulir tampil kosong beserta sebabnya, bukan galat |
| **DoD** | `FE-LAB-32` naik ✅; laporan `FE-LAB-32.md` bagian baru |

---

## 7. Yang tidak diminta

| Hal | Keadaan |
|---|---|
| Endpoint menambah Spesifik Specimen dari halaman hasil | **Tetap nol** — `LAB-DEC-098` butir 5 |
| Mengubah `PATCH /correction` menjadi mengembalikan `LabSpecimenResponse` | Tidak diminta. Layar cukup memuat ulang `by-order`; dokumennya saja yang diselaraskan dengan source (butir 7 bagian 2) |
| Migration | Nol — tabel dan indeks unik `IX_LabSpecimenDetail_SpecimenId_DetailTypeId` sudah ada |
| Izin baru | Nol |

---

## 8. Sesudah disetujui — ✅ diterapkan 2026-10-06

1. `00-interview-decisions.md`: `LAB-DEC-167` `approved` beserta butir yang dipilih.
2. `contracts/api-contract.md`: bagian 34 (`r39`) dan baris *Revision*; `contracts/validation-matrix.md`:
   `VAL-150` (`r16`).
3. `roadmap/backend-roadmap.md`: bagian susulan dengan kartu `BE-LAB-88`.
4. `roadmap/frontend-roadmap.md` dan `roadmap/traceability.md`: `FE-LAB-32` menunggu `BE-LAB-88`;
   baris `LAB-DEC-098`/`LAB-DEC-107` menyebut `r39`.
5. `BE-LAB-88` dikerjakan lewat alur task backend biasa; lalu sisa `FE-LAB-32`.

**Diterapkan:** butir 1–4 — `00-interview-decisions.md` rev 84, `api-contract.md` `r39` bagian 34,
`validation-matrix.md` `r16` bagian 18, `backend-roadmap.md` rev 94 bagian 6ap, `frontend-roadmap.md`
rev 59, `traceability.md` rev 125. Butir 5: **`BE-LAB-88` ✅ selesai 2026-10-06**
([`BE-LAB-88.md`](../task/report/backend/BE-LAB-88.md)); sisa `FE-LAB-32` **selesai** 2026-10-06 — `FE-LAB-32` ✅
([`FE-LAB-32.md`](../task/report/frontend/FE-LAB-32.md) bagian 10).

Bila salah satu butir memilih B, bagian 4–6 disesuaikan lebih dulu sebelum ditempel.
