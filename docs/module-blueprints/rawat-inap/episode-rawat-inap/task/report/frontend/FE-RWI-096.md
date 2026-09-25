# Laporan Perubahan Frontend — `FE-RWI-096`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-096` |
| Judul | Tutup jalan buntu admisi bayi baru lahir sampai kemampuannya tersedia |
| Slice | Perbaikan defect pasca-pengujian; bukan slice fitur baru |
| Roadmap | `docs/module-blueprints/rawat-inap/episode-rawat-inap/roadmap/frontend-roadmap-v2.md` |
| Trace | `ISSUE-EPS-002` butir `ISS-EPS-01`; `FE-RWI-022`; `BE-RWI-031`; `EPIC RI-33` |
| Contract version | `0.9.0` — tidak disentuh. Task ini tidak memanggil backend sama sekali |
| Dependency | Tidak ada. Sengaja **tidak** bergantung pada `BE-RWI-128` yang ditunda |
| Klasifikasi | `LIGHT` |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — source; laporan dan tautan bukti di `NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/episode-rawat-inap/**` |
| Wewenang UI | Pilihan 1 disetujui pemilik 23 September 2026 — kartu dinonaktifkan beserta badge, bukan disembunyikan |
| Model | Claude Opus 5 |
| Commit frontend saat dikerjakan | `a03676d1d829f56e40b46bdc1747498b7f5dad99` |
| Tanggal | 23 September 2026 |
| Status | Selesai untuk lingkup yang diberi wewenang. Verifikasi manual di peramban `NOT RUN` |

---

## 1. Masalah yang diperbaiki

Petugas admisi yang memilih tipe pasien **"Bayi Baru Lahir"** tidak dapat melanjutkan ke langkah
berikutnya, dan tidak diberi tahu mengapa.

Penyebabnya, panel "Episode Ibu" yang muncul sesudah kartu itu dipilih disuapi daftar pilihan yang
dikunci kosong permanen dan tidak pernah punya sumber data, sementara penjaga lanjut menuntut
episode ibu terisi. Akibatnya tombol "Lanjut ke Pembayaran" tidak akan pernah aktif. Satu-satunya
petunjuk di layar adalah kalimat teknis *"Koneksi data episode ibu berada di luar scope
FE-RWI-022"* — kalimat yang tidak berarti apa pun bagi petugas admisi.

Dari sudut pandang petugas, kartu itu terlihat sama persis seperti lima kartu lainnya. Mereka
memilihnya, lalu mentok tanpa penjelasan.

**Keputusan pemilik 23 September 2026:** pendaftaran bayi baru lahir belum masuk lingkup rilis ini.
Karena itu yang dikerjakan di sini **bukan** menyambungkan episode ibu, melainkan menutup jalan
buntunya dengan jujur.

---

## 2. Proses bisnis

**Tujuan.** Petugas admisi memilih kategori pasien pada Langkah 3 alur admisi rawat inap tanpa
terjebak pada kategori yang belum tersedia.

**Pelaku.** Petugas admisi rawat inap.

**Langkah berurutan sesudah perubahan:**

1. Petugas membuka Langkah 3 dan melihat enam kartu kategori pasien.
2. Kartu **Bayi Baru Lahir** tampil dengan badge **"Belum tersedia"** dan tidak dapat diklik.
   Deskripsinya berbunyi: *"Pendaftaran bayi baru lahir belum tersedia pada rilis ini. Hubungi admin
   sistem bila ada bayi yang perlu didaftarkan."*
3. Petugas memilih salah satu dari lima kategori yang tersedia.
4. Tombol "Lanjut ke Pembayaran" aktif, alur berjalan seperti biasa.

**Kenapa kartunya tidak disembunyikan saja.** Karena petugas yang menghadapi bayi baru lahir tetap
harus mendaftarkannya. Bila kategorinya hilang sama sekali dari layar, petugas akan memilih kategori
terdekat — biasanya "Anak" atau "Umum" — dan episode bayi yang salah kategori itu **tidak dapat
dibetulkan** sesudah bayi ditempatkan, sebagaimana tercatat pada daftar risiko
`requirement-traceability.md` sub-modul ini. Pintu yang jelas terkunci lebih murah daripada salah
klasifikasi yang diam-diam.

**Jalur tidak normal.** Tidak ada. Kartu yang dinonaktifkan tidak memicu perubahan state apa pun;
`BaseCheckboxCard` sudah memblokir `onChange` ketika `disabled` bernilai benar.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `AGENTS.md` frontend; `rules/frontend/frontend-architecture.md`; `rules/frontend/test-policy.md`
- `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx`
- `src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx`
- `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-flow.jsx`
- `src/components/features/base-features/base-checkbox-card.jsx` — memastikan `disabled` dan `badge` memang didukung
- `src/components/ui/form-pemeriksaan-ui/BaseCheckboxCard.jsx` — dipastikan **bukan** komponen yang dipakai layar ini
- Backend sebagai rujukan: `InpatientEpisodeController.cs`, `InpEpisode.cs`

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `.../constants/.../inpatient-admission-flow-constants.jsx` | Opsi `newborn` diberi `disabled: true` dan `badge: "Belum tersedia"`, deskripsinya diganti menjadi kalimat yang dimengerti petugas. `EMPTY_MOTHER_EPISODE_SELECT` dihapus dan diganti komentar yang menjelaskan mengapa, beserta rujukan ke dokumen issue |
| `.../view/.../inpatient-admission-view.jsx` | `badge` dan `disabled` diteruskan ke `BaseCheckboxCard` beserta `aria-disabled`; panel "Episode Ibu" dan `InformationAlert` teknisnya dihapus; import `ResourceFilterSelect` dan `EMPTY_MOTHER_EPISODE_SELECT` yang menjadi tidak terpakai ikut dibersihkan |

Netto `27 insertions`, `42 deletions` — perubahan ini menghapus lebih banyak daripada menambah.

### 3.3 Yang sengaja **tidak** diubah

| Hal | Alasan |
| --- | --- |
| `canContinuePatientType` pada `use-inpatient-admission-flow.jsx` | Penjaga `patientType && (!isNewborn \|\| motherEpisodeId)` dibiarkan utuh. Aturan bisnisnya benar dan tetap berlaku; menghapusnya akan menghilangkan jejak aturan itu dari kode, dan harus ditulis ulang ketika kemampuannya kembali. Dengan kartu dinonaktifkan, `isNewborn` tidak akan pernah bernilai benar, sehingga penjaga itu tidak aktif |
| `motherEpisodeId` beserta setter-nya pada hook | Alasan sama. Masih dipakai `FE-RWI-027` yang mempertahankan `motherEpisodeId` saat `PUT /episodes/{id}` |
| Backend | Task ini tidak menyentuh backend sama sekali |

### 3.4 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — tidak ada panggilan backend yang ditambah, diubah, atau dihapus |
| Database | `NOT APPLICABLE` |
| Keamanan/Auth | `NOT APPLICABLE` — tidak ada perubahan otorisasi. Penonaktifan kartu adalah pembatasan tampilan, bukan kontrol keamanan; tidak ada jalur tulis baru yang dibuka |

---

## 4. Tabel keputusan base component

`UI GATE: PASS` — seluruh elemen `REUSE`, tidak ada yang menunggu keputusan.

| Elemen | Status | Bukti |
| --- | --- | --- |
| Kartu kategori pasien | `REUSE` | `BaseCheckboxCard` sudah mendukung `disabled` (memblokir `onChange` **dan** menempelkan kelas `checkboxStyles.disabled`) serta `badge` (label ringkas di sisi kanan judul). Tidak ada prop baru, tidak ada perubahan perilaku bawaan |
| Badge "Belum tersedia" | `REUSE` | Prop `badge` bawaan komponen yang sama, sudah dipakai kartu cara bayar rawat inap |
| Panel Episode Ibu | Dihapus | Tidak digantikan apa pun |

Tidak ada komponen baru, tidak ada berkas CSS yang disentuh, dan tidak ada nilai visual literal yang
ditulis — dibuktikan lewat grep anti-regresi pada diff (`#hex`, `px`, `rgba()`, `font-size`,
`margin:`, `padding:`) yang hasilnya kosong.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `npx eslint` pada kedua berkas yang diubah | Bersih, exit code `0`, tanpa peringatan | `PASS` | Keluaran perintah |
| `npm run build` | Berhasil, exit code `0`; tabel route lengkap; `postbuild` standalone selesai — *"Standalone runtime siap dijalankan"* | `PASS` | Keluaran perintah |
| Grep anti-regresi nilai visual literal pada diff | Nol hasil | `PASS` | Keluaran perintah |
| `BaseCheckboxCard` mendukung `disabled` dan `badge` | Terkonfirmasi pada source komponen | `PASS` | `base-checkbox-card.jsx` — `disabled` memblokir `handleChange` dan menambah kelas; `badge` dirender sebagai `checkboxBadge` |
| Tidak ada sisa rujukan ke `EMPTY_MOTHER_EPISODE_SELECT` | Nol hasil di seluruh `src/` | `PASS` | Keluaran perintah |
| Kartu Bayi Baru Lahir tidak dapat diklik di peramban | Belum dijalankan | `NOT RUN` | Verifikasi peramban tidak dijalankan pada task ini |
| Lima kategori lain tetap dapat dipilih dan alur berlanjut | Belum dijalankan | `NOT RUN` | Sama seperti di atas |

`MANUAL TEST: NOT RUN` — verifikasi peramban tidak dijalankan. Sesuai kebijakan pemilik, bukti
source, lint, dan build memadai untuk menutup task ini; butir yang dilewati ditulis apa adanya.

`AUTOMATED TEST: SKIPPED (opsional) — repository ini tidak memakai Jest; menulis test baru bukan
gerbang selesai menurut rules/frontend/test-policy.md`.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Kartu "Bayi Baru Lahir" tidak dapat dipilih | Terpenuhi | `disabled: true` pada opsi, diteruskan ke `BaseCheckboxCard` yang memblokir `onChange` |
| Petugas mengetahui kategori itu ada dan sedang belum tersedia | Terpenuhi | Badge "Belum tersedia" beserta deskripsi berbahasa Indonesia yang menyebut langkah yang harus diambil petugas |
| Panel Episode Ibu beserta alert teknisnya hilang dari layar | Terpenuhi | Blok dihapus; kalimat *"di luar scope FE-RWI-022"* tidak lagi ada di source |
| Lima kategori lain tidak terpengaruh | Terpenuhi | `disabled` dan `badge` hanya ada pada opsi `newborn`; kelima opsi lain tidak disentuh |
| Tidak ada komponen atau CSS baru | Terpenuhi | `UI GATE: PASS`, seluruh elemen `REUSE`, grep nilai visual literal kosong |
| Lint dan build hijau | Terpenuhi | Keduanya `PASS` |
| Aturan bisnis bayi baru lahir tidak hilang dari kode | Terpenuhi | Penjaga `canContinuePatientType` dibiarkan utuh; acceptance criteria kemampuan penuhnya tercatat di dokumen issue |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Tidak ada peringatan lint maupun build pada berkas yang diubah |
| Masalah yang diketahui | Pendaftaran bayi baru lahir tetap **tidak dapat dilakukan** lewat sistem. Itu keadaan yang disengaja dan sudah diputuskan pemilik, bukan cacat yang tersisa. Yang berubah hanyalah keadaan itu kini dinyatakan jujur di layar alih-alih berupa jalan buntu diam-diam |
| Risiko tersisa | (1) Petugas admisi yang menghadapi bayi baru lahir belum punya jalur resmi; prosedur sementaranya perlu ditetapkan di luar sistem. (2) Verifikasi peramban belum dijalankan |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` |
| Status Git | `M src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx`, `M src/lib/constants/health-services/inpatient-management/inpatient-admission-flow-constants.jsx`. Perubahan lain pada working tree — seluruhnya milik modul hemodialisa dan `use-inpatient-prescription-tab.jsx` dari `FE-RWI-095` — tidak disentuh |
| Langkah berikutnya | (1) Tetapkan prosedur sementara pendaftaran bayi baru lahir bagi petugas admisi. (2) Ketika kemampuannya masuk rilis, kerjakan `BE-RWI-128` untuk endpoint episode ibu aktif lalu sambungkan pilihannya — acceptance criteria lengkapnya sudah tersedia pada `ISS-EPS-01` |
