# Laporan Perubahan Frontend — `FE-RAD-04`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RAD-04` |
| Judul | Layar aturan keselamatan dan papan kesiapan alat |
| Epic | `EPIC RAD-06` |
| Requirement | `FR-RAD-050` (penomoran blueprint) |
| Decision | `RAD-DEC-005` |
| Roadmap | `docs/module-blueprints/radiologi/roadmap/frontend-roadmap.md` bagian 3, gelombang `MVP-1` |
| Contract version | `RAD-API-001` grup *Master Data / Rad Safety Rule*; `RAD-STATE-001` bagian 5 |
| Acceptance criteria | `AC-17` — alat tanpa aturan aktif: layar menampilkan peringatan |
| Test yang diminta roadmap | `UAT-03` — admin mengesahkan aturannya sendiri ditolak; peringatan alat tanpa aturan aktif tampil |
| **Ketentuan mengikat** | `RAD-ARCH-FE-001` bagian 5 butir 5 — layar **wajib** menampilkan peringatan untuk setiap alat tanpa aturan aktif |
| Dependency | `FE-RAD-01` **selesai**; `BE-RAD-03` **selesai** |
| Task mode | `FRONTEND` — frontend target tulis; backend strict read-only kecuali laporan ini |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Branch frontend | `YogaV2`, upstream `origin/YogaV2` |
| Tanggal | 2026-09-11 |
| Status | **Selesai.** 18 unit test baru lulus; 797 unit test repository lulus, 0 gagal; lint 0 error |

---

## 1. Ketentuan mengikat: papan kesiapan alat

`RAD-ARCH-FE-001` bagian 5 butir 5 mewajibkan peringatan untuk setiap alat tanpa aturan aktif.
Alasannya bukan kerapian: gerbang keselamatan bersifat **fail-closed**, sehingga alat tanpa
aturan berlaku menolak **seluruh** pemeriksaannya — dan tanpa peringatan, ketiadaan itu baru
diketahui saat pasien sudah berada di depan alat.

**Layar membedakan empat keadaan**, dan pembedaannya yang menentukan:

| Keadaan | Yang ditampilkan |
| --- | --- |
| Papan **gagal dimuat** | Kotak merah berisi pesan server, ditambah kalimat "jangan menyimpulkan seluruh alat sudah tercakup" |
| Papan **belum pernah** berhasil dimuat | Kotak biru: kesiapan alat belum diketahui. **Bukan** "semua aman" |
| Ada alat belum tercakup | Kotak kuning berisi kartu per alat, masing-masing dapat ditekan untuk menyaring daftar ke alat itu |
| Berhasil dimuat dan kosong | Kotak hijau: seluruh alat sudah tercakup |

Keadaan kedua adalah yang paling mudah terlewat. `GET /coverage` hanya mengembalikan alat yang
bermasalah, sehingga balasan kosong dan permintaan yang belum pernah berhasil **terlihat sama
persis** di layar. Menyamakannya berarti menyatakan "semua alat aman" padahal tidak ada yang
pernah memeriksanya.

Setiap kartu juga membedakan **sudah dimulai** dari **belum sama sekali**, memakai
`draftOrPendingRuleCount`. Keduanya sama-sama menolak pemeriksaan hari ini, tetapi langkah
berikutnya berbeda: yang satu menunggu penanggung jawab klinis, yang lain menunggu admin.

---

## 2. Pemisahan kewenangan — `UAT-03`

`RAD-DEC-005` memisahkan penyusun dari pengesah. Layar menegakkannya di dua lapis:

| Lapis | Apa yang dikerjakan |
| --- | --- |
| **Keadaan aturan** | Tombol hanya muncul pada keadaan yang mengizinkannya: Perbarui dan Ajukan hanya pada `Draft`; Sahkan dan Tolak hanya pada `PendingApproval`; Hentikan hanya pada `Active` |
| **Kewenangan pengguna** | Sahkan, Tolak, Hentikan, dan Ajukan hanya tampil bagi pemegang `RadSafetyRule : Approve`, `: Reject`, `: Deactivate`, dan `: Submit` lewat `usePermission` |

**Keduanya bantuan tampilan, bukan pengaman.** Backend memeriksa ulang keadaan dan kewenangan
pada setiap endpoint. Ketika penolakan `403` tetap terjadi — inti `UAT-03` — pesannya
ditampilkan apa adanya dan **modal konfirmasi tidak ditutup**, supaya penolakan "Hanya
penanggung jawab klinis yang boleh mengesahkan aturan keselamatan" terbaca pada konteks aturan
yang sama.

Modal pengesahan juga memuat keterangan tetap: bila tombol Sahkan tidak muncul, akunnya belum
memegang kewenangan itu. Pengguna karena itu tahu sebelum mencari-cari.

---

## 3. Perubahan yang dikerjakan

| Berkas | Keadaan |
| --- | --- |
| `src/lib/constants/health-services/master-data/rad-safety-rules/rad-safety-rule-constants.jsx` | Baru — 6 ruas formulir, 6 kartu ringkasan, 6 kolom, 5 penyaring, meta 4 keadaan |
| `src/utils/health-services/master-data/rad-safety-rules/rad-safety-rule-utils.jsx` | Baru — fungsi murni, termasuk lima `can*` dan `mapCoverageRows` |
| `src/lib/state/slice/health-services/master-data/master-data-rad-safety-rule-slice.jsx` | Baru — 12 thunk, 3 reducer, 26 selector |
| `.../hooks/…/use-master-data-rad-safety-rule.jsx` | Baru — controller daftar, papan kesiapan, dan empat aksi siklus |
| `.../hooks/…/use-master-data-rad-safety-rule-editor.jsx` | Baru — controller formulir draf |
| `.../view/…/master-data-rad-safety-rule-view.jsx` | Baru — layar daftar dan papan kesiapan |
| `.../view/…/add/rad-safety-rule-form-view.jsx` | Baru — formulir draf |
| `src/style/health-services/master-data/rad-safety-rules/rad-safety-rule.module.css` | Baru — 8 kelas, design token |
| `src/app/health-services/master-data/rad-safety-rules/…` | Baru — 5 berkas route |
| `src/lib/state/store.jsx` | Diubah — 2 baris |
| `src/utils/menu-sidebar/menu-items.jsx` | Diubah — 1 entri menu |
| `tests/unit/rad-safety-rule-rules.test.mjs` | Baru — 18 test |

---

## 4. Empat keputusan teknis

### 4.1 Formulir menyatakan lebih dulu bahwa aturannya tidak dapat diubah

Aturan berstatus `PendingApproval`, `Active`, atau `Inactive` dibuka sebagai bacaan, dan
kotak peringatan menyebut keadaannya beserta alasannya. Tanpa itu, admin mengetik perubahan
panjang lalu ditolak `403` saat menyimpan.

### 4.2 Alasan penolakan terakhir ikut ditampilkan pada formulir

Backend sengaja tidak menghapusnya ketika aturan diajukan ulang — jejak penolakan termasuk
audit. Menampilkannya membuat penyusun tahu persis apa yang harus diperbaiki.

### 4.3 Penyaring keadaan memakai angka, balasan memakai teks

`RadSafetyRulePagedQuery.RuleStatus` menerima enum sebagai angka, sedangkan
`RadSafetyRuleResponse.RuleStatus` mengirim teks. Keduanya sengaja **tidak** disamakan di
frontend: yang berlaku adalah apa yang benar-benar diterima dan dikirim backend, bukan bentuk
yang paling nyaman bagi layar. Perbedaannya dicatat pada komentar hook.

### 4.4 Pilihan alat dan butir dibaca dari service milik grupnya

Aturan keselamatan adalah penghubung antara alat dan butir. Menyalin daftar alat ke potongan
Redux ini akan membuat dua sumber kebenaran untuk pertanyaan "alat apa saja yang ada".
`getRadModalityOptions` dan `getRadSafetyRequirementOptions` dipanggil dari sini.

---

## 5. Validasi

| Yang dijalankan | Hasil |
| --- | --- |
| `node --test tests/unit/rad-safety-rule-rules.test.mjs` | **18 lulus, 0 gagal** |
| `node --test tests/unit/` (seluruh repository) | **797 lulus, 0 gagal** |
| `npx eslint` atas seluruh berkas yang disentuh | **0 error, 2 warning** |
| `npm run build` | **Compiled successfully in 37.8s.** Ketiga route terdaftar: `/health-services/master-data/rad-safety-rules` (static), `/create` (static), `/[slug]/update` (dynamic) |

Satu warning ketiga sempat muncul — `react-hooks/preserve-manual-memoization`, yang membuat
React Compiler melewatkan optimasi seluruh hook daftar — dan **sudah diperbaiki** dengan
mengeluarkan `listState?.items` menjadi variabel lebih dulu sehingga dependensinya cocok. Dua
warning tersisa `react-hooks/set-state-in-effect`, sama profilnya dengan hook rujukan
repository.

**`MANUAL TEST: NOT FEASIBLE`** untuk `UAT-03` jalur penolakan. Menjalankannya menuntut dua
akun berbeda — satu admin penyusun, satu penanggung jawab klinis — dan penunjukan penanggung
jawab klinis itu sendiri masih terbuka: `RAD-OPEN-001` belum ditunjuk, `DEC-RAD-005` masih
`OPEN`. Tidak ada akun yang memegang `RadSafetyRule : Approve` untuk diuji melawan. Jalur
tampilan yang diuji `AC-17` — peringatan alat tanpa aturan aktif — dikunci unit test
`FE-RAD-04 S6` dan `S7`.

---

## 6. Yang sengaja **tidak** dikerjakan

| Butir | Alasan |
| --- | --- |
| Penyempitan aturan ke satu pemeriksaan (`ProcedureId`) | `CreateRadSafetyRuleRequest` menerimanya dan mengosongkannya berarti aturan berlaku untuk **seluruh** pemeriksaan pada alat itu — konfigurasi yang lebih ketat, bukan lebih longgar. Tetapi balasan tidak memuat kode maupun nama pemeriksaan, dan tidak ada endpoint pilihan pemeriksaan radiologi; membuat pickernya berarti menarik master data milik modul lain tanpa penyaring `IsRadiology`. Ditunda beserta catatan ini |
| Halaman detail aturan | Rincian dibaca formulir lewat `GET /{id}`, sama seperti dua fitur radiologi sebelumnya |
| Tombol Hapus | Grup ini **tidak punya** `DELETE`. Aturan dihentikan, bukan dihapus — riwayat pengesahan termasuk audit |
| Base component baru | Dilarang `AGENTS.md` |

---

## 7. Risiko dan dependency yang masih terbuka

| Hal | Keadaan |
| --- | --- |
| Penanggung jawab klinis belum ditunjuk | `RAD-OPEN-001` terbuka. **Selama ini belum terjadi, tidak ada aturan yang dapat disahkan** — dan karena itu modul Radiologi tetap menolak seluruh pemeriksaan. Layar ini sudah siap; yang belum ada adalah orang yang berwenang menekan Sahkan |
| `DEC-RAD-005` `OPEN` | Menahan `BE-RAD-15` menyelesaikan pengisian aturan, dan menahan verifikasi manual `UAT-03`, `UAT-12`, serta jalur penolakan `FE-RAD-03` |
| Papan kesiapan dihitung backend | Layar tidak menghitung sendiri; bila `GET /coverage` berubah bentuk, peringatannya ikut berubah. Itu disengaja — satu sumber kebenaran |

---

## 8. Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 1 | 2026-09-11 | Laporan dibuat. 13 berkas baru, 2 diubah. 18 test baru lulus; 797 test repository lulus; lint 0 error. Gelombang `MVP-1` selesai. | `draft` |
