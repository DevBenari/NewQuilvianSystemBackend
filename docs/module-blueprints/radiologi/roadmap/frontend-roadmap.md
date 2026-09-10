# Frontend Roadmap — Modul Radiologi

| Field | Value |
|---|---|
| Roadmap ID | `RAD-RM-FE-001` |
| Revision | `1` |
| Status | `approved` |
| Blueprint | `RAD-BP-001` revision 9, status `approved` |
| Frontend SHA baseline | `f66ed1885` |
| Kontrak terkunci | `RAD-ARCH-FE-001`, `RAD-API-001`, `RAD-STATE-001`, `RAD-VAL-001`, `RAD-PERM-001` — seluruhnya `approved` 2026-09-10 |
| Owner | Yoga Aji Pratama |
| `approved_by` / `approved_at` | Yoga Aji Pratama, 2026-09-10 |
| Tanggal | 2026-09-10 |

> **Keadaan awal.** Frontend Radiologi **belum ada sama sekali** (`RAD-CAP-029`). Tidak ada satu
> pun halaman, service, atau state yang memanggil endpoint radiologi. Seluruhnya dibangun dari
> nol, mengikuti pola modul Laboratorium yang sudah terbukti berjalan.

> **Batas dokumen ini.** Roadmap bukan izin menulis kode. Setiap task memerlukan wewenang
> `FRONTEND MODE` tersendiri saat dikerjakan.

---

## 1. Ringkasan

13 task frontend. Setiap task menghasilkan layar yang dapat dipakai orang, bukan potongan
lapisan.

| Gelombang | Task | Epic | Hasil yang dapat dipakai |
|---|---|---|---|
| `MVP-1` | `FE-RAD-01` s/d `FE-RAD-04` | `EPIC RAD-06` | Admin dapat mendaftarkan alat dan mengesahkan aturan keselamatan |
| `MVP-3` | `FE-RAD-05` s/d `FE-RAD-12` | `EPIC RAD-05`, `RAD-02`, `RAD-03` | Satu pasien dapat berjalan dari pesanan sampai hasil dirilis |
| `MVP-4` | `FE-RAD-13` | `EPIC RAD-04`, `RAD-07` | Dokter membaca hasil dari rekam medis; radiografer memakai daftar kerja |

**Paralel dengan backend diizinkan** karena seluruh contract version sudah `approved` dan
hash-nya terkunci pada `blueprint-manifest.md` revision 9.

---

## 2. Kewenangan UI

Sebagian besar keputusan tampilan berstatus `DEV_DISCRETION`, karena **tidak ada brief UI yang
disetujui** untuk Radiologi. Syaratnya mengikuti pola Laboratorium dan design token project.

Yang **bukan** `DEV_DISCRETION` adalah sembilan ketentuan mengikat pada `RAD-ARCH-FE-001`
bagian 5. Setiap task di bawah menyebut ketentuan mana yang berlaku padanya.

---

## 3. Gelombang `MVP-1` — Pengelolaan Data Induk

### `FE-RAD-01` — Fondasi modul

| Field | Isi |
|---|---|
| Epic | `EPIC RAD-06` |
| Contract | `RAD-API-001`, `RAD-ARCH-FE-001` bagian 2 |
| Yang dikerjakan | Service Axios, Redux slice, konstanta, dan kerangka route mengikuti pola `laboratory-management`. Belum ada layar berisi |
| Acceptance criteria | Pemanggilan endpoint radiologi berhasil lewat service; penanganan galat mengikuti pola yang ada |
| Test | Unit service dan slice |
| Dependency | `BE-RAD-03` tersedia di lingkungan pengembangan |
| Risiko | Rendah. **Jangan** membuat Axios instance baru atau abstraksi generik |
| Owner | Frontend |
| Definition of Done | Struktur folder sesuai pola Laboratorium; tidak ada base component baru |

### `FE-RAD-02` — Layar kelola alat pencitraan

| Field | Isi |
|---|---|
| Epic | `EPIC RAD-06` |
| Requirement | `FR-RAD-051` |
| Contract | `RAD-API-001` grup *Master Data / Rad Modality*; `RAD-VAL-001` bagian 3 |
| Acceptance criteria | AC-5 |
| Test | `UAT-12` — menonaktifkan alat yang masih dipakai ditolak beserta pesannya |
| Dependency | `FE-RAD-01`, `BE-RAD-04` |
| Ketentuan mengikat | — |
| Risiko | Rendah |
| Owner | Frontend |
| Definition of Done | Alat baru dapat didaftarkan lewat layar, tanpa menyentuh database |

### `FE-RAD-03` — Layar kelola butir keselamatan

| Field | Isi |
|---|---|
| Epic | `EPIC RAD-06` |
| Contract | `RAD-API-001` grup *Master Data / Rad Safety Requirement* |
| Acceptance criteria | Turunan AC-5 |
| Test | Butir yang masih dipakai tidak dapat dinonaktifkan |
| Dependency | `FE-RAD-01`, `BE-RAD-05` |
| Risiko | Rendah |
| Owner | Frontend |
| Definition of Done | Butir keselamatan dapat dikelola lewat layar |

### `FE-RAD-04` — Layar aturan keselamatan dan papan kesiapan alat

| Field | Isi |
|---|---|
| Epic | `EPIC RAD-06` |
| Requirement | `FR-RAD-050` |
| Decision | `RAD-DEC-005` |
| Contract | `RAD-API-001` grup *Master Data / Rad Safety Rule*; `RAD-STATE-001` bagian 5 |
| Yang dikerjakan | Susun draf, ajukan pengesahan, sahkan, tolak, nonaktifkan. **Papan kesiapan alat** dari `GET /coverage` |
| Acceptance criteria | AC-17 |
| Test | `UAT-03` — admin mengesahkan aturannya sendiri ditolak; peringatan alat tanpa aturan aktif tampil |
| Dependency | `FE-RAD-01`, `BE-RAD-03` |
| **Ketentuan mengikat** | Bagian 5 butir 5 — layar **wajib** menampilkan peringatan untuk setiap alat tanpa aturan aktif |
| Risiko | Sedang. Tanpa peringatan itu, ketiadaan aturan baru diketahui saat pasien sudah di depan alat |
| Owner | Frontend |
| Definition of Done | Peringatan tampil; tombol Sahkan tidak muncul bagi yang tidak berwenang |

---

## 4. Gelombang `MVP-3` — Alur Pemeriksaan dan Hasil Bacaan

### `FE-RAD-05` — Buat pesanan radiologi

| Field | Isi |
|---|---|
| Epic | `EPIC RAD-05`, `RAD-07` |
| Requirement | `FR-RAD-042`, `FR-RAD-064` |
| Decision | `RAD-DEC-013` |
| Contract | `RAD-API-001` grup *Rad Order* |
| Yang dikerjakan | Layar dokter memesan pemeriksaan, termasuk **penanda cito** |
| Acceptance criteria | AC-42 |
| Test | Tombol Simpan ditekan dua kali hanya menghasilkan satu pesanan |
| Dependency | `FE-RAD-01`, `BE-RAD-12` |
| **Ketentuan mengikat** | Bagian 9 — pencegahan kiriman ganda |
| Risiko | Rendah |
| Owner | Frontend |
| Definition of Done | Pesanan terkirim sekali; penanda cito tersimpan |

### `FE-RAD-06` — Antrian pesanan masuk

| Field | Isi |
|---|---|
| Epic | `EPIC RAD-05` |
| Contract | `RAD-API-001` grup *Rad Order*; `RAD-STATE-001` bagian 1 |
| Yang dikerjakan | Layar petugas pendaftaran radiologi: terima, jadwalkan, tolak, batalkan |
| Acceptance criteria | Transisi sesuai `RAD-STATE-001` bagian 1 |
| Test | Tindakan yang tidak sah tidak ditawarkan di layar |
| Dependency | `FE-RAD-01` |
| Risiko | Rendah |
| Owner | Frontend |
| Definition of Done | Seluruh transisi sah dapat dijalankan dari layar |

### `FE-RAD-07` — Daftar kerja per alat

| Field | Isi |
|---|---|
| Epic | `EPIC RAD-07` |
| Requirement | `FR-RAD-060`, `FR-RAD-062`, `FR-RAD-063` |
| Decision | `RAD-DEC-012`, `RAD-DEC-013` |
| Contract | `RAD-API-001` endpoint `GET /worklist` |
| Yang dikerjakan | Layar radiografer memilih alat lalu melihat pekerjaan hari itu |
| Acceptance criteria | AC-36, AC-38, AC-40 |
| Test | `UAT-13` — pesanan cito di urutan pertama; `UAT-14` — daftar kerja tanpa memilih alat ditolak |
| Dependency | `FE-RAD-01`, `BE-RAD-13` |
| **Ketentuan mengikat** | Bagian 5 butir 9 — penanda cito wajib terlihat tanpa membuka rincian, dan pesanan cito wajib di urutan atas |
| Risiko | Sedang. Cito yang tenggelam di tengah daftar sama saja dengan tidak ditandai |
| Owner | Frontend |
| Definition of Done | Petugas dapat berpindah antar daftar alat tanpa berganti halaman |

### `FE-RAD-08` — Verifikasi pasien dan gerbang keselamatan

| Field | Isi |
|---|---|
| Epic | `EPIC RAD-05` |
| Requirement | `FR-RAD-040` |
| Decision | `RAD-DEC-002` |
| Contract | `RAD-API-001` grup *Rad Study*; `RAD-VAL-001` bagian 4 |
| Yang dikerjakan | Layar verifikasi identitas; isian butir keselamatan yang **mengikuti alat** |
| Acceptance criteria | AC-6 |
| Test | `UAT-10` — butir MRI dan CT-Scan berbeda; `UAT-02` — alat tanpa aturan aktif ditolak beserta pesannya |
| Dependency | `FE-RAD-01`, `BE-RAD-06` |
| **Ketentuan mengikat** | Bagian 10 — isian wajib dapat dioperasikan dengan papan ketik, karena radiografer sering memakai sarung tangan |
| Risiko | **Menyentuh keselamatan pasien.** Butir yang ditampilkan wajib berasal dari master, **jangan** ditanam di frontend |
| Owner | Frontend |
| Definition of Done | Butir mengikuti alat; pesan penolakan menyebut butir yang menahan |

### `FE-RAD-09` — Pengambilan citra, mutu, dan bahan terpakai

| Field | Isi |
|---|---|
| Epic | `EPIC RAD-05` |
| Requirement | `FR-RAD-041` |
| Contract | `RAD-API-001` grup *Rad Study*; `RAD-STATE-001` bagian 2 |
| Yang dikerjakan | Mulai, selesai, hentikan acquisition; nilai mutu; catat bahan terpakai; tampilkan penanda pengulangan |
| Acceptance criteria | AC-41 |
| Test | Penanda "Pengulangan dari study ke-1" beserta sebabnya terlihat di daftar study |
| Dependency | `FE-RAD-08` |
| **Ketentuan mengikat** | Bagian 5 butir 7 — penanda pengulangan wajib terlihat |
| Risiko | Sedang. Petugas perlu tahu pasien sudah pernah disinari sebelumnya |
| Owner | Frontend |
| Definition of Done | Seluruh transisi study dapat dijalankan; penilaian mutu tidak dapat dikirim ganda |

### `FE-RAD-10` — Daftar bacaan menunggu dan penulisan draf

| Field | Isi |
|---|---|
| Epic | `EPIC RAD-02` |
| Requirement | `FR-RAD-010` |
| Contract | `RAD-API-001` grup *Rad Report*; `RAD-VAL-001` bagian 1 |
| Yang dikerjakan | Daftar study layak yang belum dibaca; layar tulis dan ubah draf |
| Acceptance criteria | Kesimpulan wajib diisi; draf hanya dapat diubah penulisnya |
| Test | Menyimpan draf tanpa kesimpulan ditolak beserta pesannya |
| Dependency | `FE-RAD-01`, `BE-RAD-09` |
| **Ketentuan mengikat** | Bagian 5 butir 6 — isi bacaan **tidak boleh** disimpan di penyimpanan browser |
| Risiko | Sedang. Data medis |
| Owner | Frontend |
| Definition of Done | Tidak ada isi bacaan tersisa di penyimpanan browser setelah halaman ditutup |

### `FE-RAD-11` — Pengesahan dan perilisan bacaan

| Field | Isi |
|---|---|
| Epic | `EPIC RAD-02` |
| Requirement | `FR-RAD-011`, `FR-RAD-012` |
| Decision | `RAD-DEC-003`, `RAD-DEC-015` |
| Contract | `RAD-STATE-001` bagian 3; `RAD-PERM-001` bagian 5.1 |
| Yang dikerjakan | Tombol Sahkan dan Rilis; penyembunyian tombol sesuai kewenangan |
| Acceptance criteria | AC-1, AC-2 |
| Test | `UAT-04` radiolog mengesahkan sendiri berhasil; `UAT-05` residen mengesahkan sendiri ditolak; `UAT-11` dua radiolog bersamaan |
| Dependency | `FE-RAD-10`, `BE-RAD-08` |
| **Ketentuan mengikat** | Bagian 5 butir 1 — tombol Sahkan wajib disembunyikan atau dinonaktifkan bagi penulis draf yang bukan radiolog |
| Risiko | **Inti keselamatan.** Penyembunyian tombol hanya membantu; penolakan sebenarnya tetap di backend |
| Owner | Frontend |
| Definition of Done | Tombol tersembunyi bagi yang tidak berwenang; pesan `409` konkurensi ditangani |

### `FE-RAD-12` — Koreksi berversi dan riwayat versi

| Field | Isi |
|---|---|
| Epic | `EPIC RAD-03` |
| Requirement | `FR-RAD-020`, `FR-RAD-021` |
| Contract | `RAD-API-001` endpoint `/amendments` dan `/versions` |
| Yang dikerjakan | Layar tulis koreksi; tampilan riwayat versi beserta alasan koreksinya |
| Acceptance criteria | AC-18 |
| Test | `UAT-06` versi 1 masih dapat dibuka dan isinya tidak berubah; `UAT-07` mengubah versi rilis ditolak |
| Dependency | `FE-RAD-11`, `BE-RAD-10` |
| **Ketentuan mengikat** | Bagian 5 butir 8 — alasan koreksi wajib ditampilkan bersama versi bacaan |
| Risiko | Sedang. Pembaca harus tahu mengapa hasilnya berubah |
| Owner | Frontend |
| Definition of Done | Riwayat versi terbaca; cara menampilkannya `DEV_DISCRETION` |

---

## 5. Gelombang `MVP-4` — Penyajian ke Rekam Medis

### `FE-RAD-13` — Hasil bacaan di rekam medis

| Field | Isi |
|---|---|
| Epic | `EPIC RAD-04` |
| Requirement | `FR-RAD-030`, `FR-RAD-031`, `FR-RAD-032` |
| Decision | `RAD-DEC-006` |
| Contract | `RAD-INT-001` bagian 2; `RAD-API-001` endpoint `GET /by-encounter` |
| Yang dikerjakan | Penyajian hasil bacaan pada rekam medis dan CPPT, **dibaca langsung** tanpa salinan |
| Acceptance criteria | AC-19, AC-20 |
| Test | `UAT-08` gangguan menampilkan pesan, bukan daftar kosong; `UAT-09` koreksi langsung terlihat |
| Dependency | `FE-RAD-12`, `BE-RAD-11` |
| **Ketentuan mengikat** | Bagian 5 butir 2, 3, dan 4 — draf tidak tampil ke dokter pengirim; versi yang tampil wajib versi berlaku; gangguan wajib dibedakan dari kekosongan |
| Risiko | **Tertinggi di roadmap frontend.** Daftar kosong terbaca "pasien tidak punya pemeriksaan" — kesimpulan salah yang berbahaya |
| Owner | Frontend |
| Definition of Done | Dua keadaan dibedakan dengan pesan berbeda; tidak ada penyimpanan sementara isi bacaan |

---

## 6. Peta Dependency

```text
FE-RAD-01 ─┬─> FE-RAD-02
           ├─> FE-RAD-03
           ├─> FE-RAD-04
           ├─> FE-RAD-05
           ├─> FE-RAD-06
           ├─> FE-RAD-07
           ├─> FE-RAD-08 ──> FE-RAD-09
           └─> FE-RAD-10 ──> FE-RAD-11 ──> FE-RAD-12 ──> FE-RAD-13
```

---

## 7. Ketentuan Mengikat dan Task yang Menanggungnya

Sembilan ketentuan pada `RAD-ARCH-FE-001` bagian 5 tidak boleh diserahkan ke selera developer.

| No | Ketentuan | Task penanggung |
|---:|---|---|
| 1 | Tombol Sahkan disembunyikan bagi penulis bukan-radiolog | `FE-RAD-11` |
| 2 | Bacaan belum dirilis tidak tampil ke dokter pengirim | `FE-RAD-13` |
| 3 | Versi yang tampil wajib versi berlaku | `FE-RAD-13` |
| 4 | Gangguan dibedakan dari kekosongan | `FE-RAD-13` |
| 5 | Peringatan alat tanpa aturan aktif | `FE-RAD-04` |
| 6 | Isi bacaan tidak disimpan di penyimpanan browser | `FE-RAD-10` |
| 7 | Penanda pengulangan terlihat | `FE-RAD-09` |
| 8 | Alasan koreksi ditampilkan bersama versi | `FE-RAD-12` |
| 9 | Penanda cito terlihat dan di urutan atas | `FE-RAD-07` |

---

## 8. Yang Tidak Ada di Roadmap Ini

| Yang tidak dikerjakan | Alasan |
|---|---|
| Layar temuan kritis dan kotak pemberitahuan | `S11` menunggu `DEC-RAD-002` |
| Layar pelewatan gerbang keselamatan darurat | `S5` menunggu `DEC-RAD-001` |
| Daftar pantau keterlambatan pesanan cito | Ditunda `RAD-DEC-013`; `RAD-OPEN-009` |
| Layar penugasan petugas ke pemeriksaan | Ditolak `RAD-DEC-012` — daftar kerja per alat, bukan per orang |
| Penampil citra | PACS dan DICOM di luar scope |
| Base component atau hook generik baru | Dilarang `AGENTS.md` frontend |

---

## 9. Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 1 | 2026-09-10 | Roadmap frontend pertama. 13 task, 3 gelombang, dari 7 epic yang disetujui. | `draft` |
