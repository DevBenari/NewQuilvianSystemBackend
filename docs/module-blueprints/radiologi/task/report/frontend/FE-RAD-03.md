# Laporan Perubahan Frontend — `FE-RAD-03`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RAD-03` |
| Judul | Layar kelola butir keselamatan |
| Epic | `EPIC RAD-06` |
| Roadmap | `docs/module-blueprints/radiologi/roadmap/frontend-roadmap.md` bagian 3, gelombang `MVP-1` |
| Contract version | `RAD-API-001` grup *Master Data / Rad Safety Requirement*; `RAD-VAL-001` bagian 3 |
| Acceptance criteria | Turunan `AC-5` — butir baru dapat didaftarkan lewat antarmuka, tanpa menyentuh database |
| Test yang diminta roadmap | Butir yang masih dipakai tidak dapat dinonaktifkan |
| Wewenang UI | Seluruhnya `DEV_DISCRETION` sepanjang mengikuti pola master data. Tidak ada ketentuan mengikat `RAD-ARCH-FE-001` bagian 5 yang berlaku |
| Dependency | `FE-RAD-01` **selesai**; `BE-RAD-05` **selesai** |
| Task mode | `FRONTEND` — frontend target tulis; backend strict read-only kecuali laporan ini |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Branch frontend | `YogaV2`, upstream `origin/YogaV2` |
| Tanggal | 2026-09-11 |
| Status | **Selesai.** 13 unit test baru lulus; 779 unit test repository lulus, 0 gagal; lint 0 error |

---

## 1. Delta kontrak yang ditemukan, dan apa yang **tidak** dibuat karenanya

`RadSafetyRequirementResponse` — bentuk satu baris daftar — **tidak memuat**:

| Ruas | Tersedia di mana |
| --- | --- |
| `sortOrder` | hanya `GET /{id}` |
| `activeRuleCount` | hanya `GET /{id}` |
| penanda "sedang dipakai aturan berlaku" | tidak ada sama sekali pada baris; hanya sebagai **penyaring** `isUsedByActiveRule` dan sebagai angka ringkasan |

Diverifikasi langsung pada proyeksi `RadSafetyRequirementService.GetPagedAsync`, bukan
disimpulkan dari nama DTO.

**Akibatnya kolom "Urutan" dan kolom "Pemakaian" sengaja tidak dibuat.** Membuatnya akan
menghasilkan kolom yang seluruh barisnya kosong — dan kolom pemakaian yang kosong jauh lebih
buruk daripada tidak ada: ia terbaca "butir ini tidak dipakai", tepat pada pertanyaan yang
menentukan apakah butir boleh dinonaktifkan.

Sebagai gantinya, pemakaian dilayani dua jalan yang memang punya datanya: **penyaring
Pemakaian** pada daftar, dan **peringatan pada formulir ubah** yang membaca `activeRuleCount`
dari `GET /{id}`. Daftar ruas yang absen dicatat eksplisit pada
`RAD_MODALITY_CONFIG`-sejenis `fieldsAbsentFromList`, dan satu unit test menjaga agar tidak
ada yang menambahkannya sebagai kolom di kemudian hari.

---

## 2. Proses bisnis dari sisi pengguna

1. Membuka **Data Master → Butir Keselamatan Radiologi**.
2. Melihat enam angka ringkasan, termasuk **"Dipakai Aturan Berlaku"** — butir yang
   benar-benar ditanyakan kepada pasien hari ini — dan **"Belum Dipakai"**.
3. Menyaring menurut kelompok, status, pemakaian, catatan wajib, dan kata kunci.
4. Menambah, memperbarui, mengaktifkan, menonaktifkan, atau menandai butir terhapus.

| Keadaan | Yang dialami pengguna |
| --- | --- |
| Membuka layar | Kotak keterangan biru menegaskan menambah butir **belum** membuatnya berlaku bagi pasien mana pun — yang mengikat adalah aturan keselamatan yang disahkan |
| Menonaktifkan butir yang masih dipakai aturan berlaku | Toast merah berisi pesan server apa adanya: "Butir ini masih dipakai aturan keselamatan yang berlaku. Nonaktifkan aturannya lebih dulu." Modal konfirmasi **tidak ditutup** |
| Menambah butir dengan kode yang sudah dipakai | Toast merah: "Kode butir keselamatan sudah dipakai." |
| Membuka formulir ubah butir yang sedang dipakai | Kotak peringatan kuning menyebut **berapa** aturan berlaku yang memakainya, dan bahwa mengubah namanya mengubah pertanyaan yang dibaca petugas saat memeriksa pasien |
| Butir berhasil ditambahkan | Toast hijau **mengingatkan** butir ini belum ditanyakan kepada siapa pun sampai disusun menjadi aturan yang disahkan |
| Daftar kosong setelah berhasil dimuat | "Tidak ada butir yang cocok dengan penyaring ini." |
| Daftar kosong karena belum pernah berhasil dimuat | "Data butir keselamatan belum dapat dimuat. Ini bukan berarti belum ada butir terdaftar." |

---

## 3. Perubahan yang dikerjakan

| Berkas | Keadaan |
| --- | --- |
| `src/lib/constants/health-services/master-data/rad-safety-requirements/rad-safety-requirement-constants.jsx` | Baru — 7 ruas formulir, 6 kartu ringkasan, 7 kolom, 5 penyaring |
| `src/utils/health-services/master-data/rad-safety-requirements/rad-safety-requirement-utils.jsx` | Baru — fungsi murni, termasuk `getActiveRuleCount` dan `buildCategoryOptions` |
| `src/lib/state/slice/health-services/master-data/master-data-rad-safety-requirement-slice.jsx` | Baru — 7 thunk, 3 reducer, 19 selector |
| `.../hooks/…/use-master-data-rad-safety-requirement.jsx` | Baru — controller daftar |
| `.../hooks/…/use-master-data-rad-safety-requirement-editor.jsx` | Baru — controller formulir |
| `.../view/…/master-data-rad-safety-requirement-view.jsx` | Baru — layar daftar |
| `.../view/…/add/rad-safety-requirement-form-view.jsx` | Baru — formulir tambah dan ubah |
| `src/style/health-services/master-data/rad-safety-requirements/rad-safety-requirement.module.css` | Baru — 1 kelas, design token |
| `src/app/health-services/master-data/rad-safety-requirements/…` | Baru — 5 berkas route |
| `src/lib/state/store.jsx` | Diubah — 2 baris |
| `src/utils/menu-sidebar/menu-items.jsx` | Diubah — 1 entri menu |
| `tests/unit/rad-safety-requirement-rules.test.mjs` | Baru — 13 test |

---

## 4. Tiga keputusan teknis

### 4.1 Kelompok butir diturunkan dari data, bukan daftar tetap

`Category` adalah teks bebas. Backend mengumumkan kelompok yang **benar-benar dipakai data
saat ini** lewat `GET /filters/metadata`. Daftar tetap yang ditanam di frontend akan meleset
begitu admin mengetik kelompok baru, dan penyaringnya diam-diam tidak pernah menemukan apa pun.

### 4.2 `activeRuleCount` yang belum diketahui dikembalikan `null`, bukan `0`

Nol berarti "tidak dipakai"; tidak diketahui berarti pertanyaannya belum dijawab. Menyamakan
keduanya membuat butir yang sedang menahan pemeriksaan terlihat aman dinonaktifkan. Satu unit
test mengunci perbedaan itu.

### 4.3 CSS module tidak dipakai ulang dari fitur Alat Pencitraan

Isinya kebetulan sama persis — satu kelas `rowActions`. Memakai ulang akan menyandera kedua
fitur: mengubah jarak tombol pada satu layar diam-diam mengubahnya pada layar lain. Lima baris
CSS lebih murah daripada kopling itu.

---

## 5. Validasi

| Yang dijalankan | Hasil |
| --- | --- |
| `node --test tests/unit/rad-safety-requirement-rules.test.mjs` | **13 lulus, 0 gagal** |
| `node --test tests/unit/` (seluruh repository) | **779 lulus, 0 gagal** |
| `npx eslint` atas seluruh berkas yang disentuh | **0 error, 2 warning** |
| `npm run build` | **Compiled successfully in 33.2s.** Ketiga route terdaftar: `/health-services/master-data/rad-safety-requirements` (static), `/create` (static), `/[slug]/update` (dynamic) |

Kedua warning `react-hooks/set-state-in-effect` sama jenis dan sama jumlahnya dengan hook
rujukan `use-master-data-lab-rejection-reason-editor.jsx`; pola yang sudah ada di repository.

**`MANUAL TEST: NOT FEASIBLE`** untuk jalur penolakan penonaktifan. Memicunya menuntut butir
yang sedang dipakai aturan keselamatan **berlaku**, sedangkan `BE-RAD-15` mencatat aturan baru
tersusun sebagai draf — `DEC-RAD-005` masih `OPEN` dengan pemilik tata kelola klinis. Tidak ada
satu pun aturan `Active` di lingkungan mana pun. Jalur sukses tidak terhalang; jalur
penolakannya dikunci unit test dan pesan servernya ditampilkan apa adanya.

---

## 6. Yang sengaja **tidak** dikerjakan

| Butir | Alasan |
| --- | --- |
| Kolom "Urutan" dan kolom "Pemakaian" pada daftar | Balasan daftar backend tidak memuat datanya — lihat bagian 1 |
| Halaman detail | Rincian dibaca formulir ubah lewat `GET /{id}`, sama seperti fitur master data sejenis |
| Saklar aktif/nonaktif pada formulir tambah | Butir baru lahir aktif dengan bawaan backend; penyalaan dan pematian lewat jalur yang punya penjaga `409`-nya |
| Base component baru | Dilarang `AGENTS.md` |

---

## 7. Risiko dan dependency yang masih terbuka

| Hal | Keadaan |
| --- | --- |
| Tidak ada aturan keselamatan `Active` | `DEC-RAD-005` `OPEN`. Menahan verifikasi manual jalur penolakan, dan menahan modul dipakai sama sekali |
| `FE-RAD-04` belum ada | Layar aturan keselamatan adalah yang akhirnya membuat butir-butir ini mengikat. Sampai itu ada, seluruh butir di layar ini tidak pernah ditanyakan kepada siapa pun — dan layar sudah menyatakan itu apa adanya |

---

## 8. Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 1 | 2026-09-11 | Laporan dibuat. 13 berkas baru, 2 diubah. 13 test baru lulus; 779 test repository lulus; lint 0 error. | `draft` |
