# ISSUE-002 — Admisi bayi baru lahir buntu, dan laporan pengujiannya menyatakan lulus

```yaml
issue_id: ISSUE-EPS-002
module_id: rawat-inap
submodule: episode-rawat-inap
sumber_temuan: testing/laporan-testing-semua-tipe-pasien.md
tanggal_issue: "2026-09-23"
status: OPEN
write_authority: BELUM DIBERIKAN
task_id_backend: BELUM DIALOKASIKAN   # deret bebas sebenarnya: BE-RWI-128
task_id_frontend: BELUM DIALOKASIKAN  # deret bebas sebenarnya: FE-RWI-096
verifikasi_source: "2026-09-23 — seluruh butir dicek langsung ke source backend dan frontend"
```

## 1. Latar belakang

`testing/laporan-testing-semua-tipe-pasien.md` menyatakan
**"✅ 100% SUKSES (ALL 6 TYPES PASSED)"** untuk pengujian enam tipe pasien pada Langkah 3 alur
admisi rawat inap.

Penelusuran ke source pada 23 September 2026 menunjukkan kesimpulan itu tidak didukung isinya.
Satu dari enam tipe — **Bayi Baru Lahir** — tidak dapat menyelesaikan alurnya sama sekali, dan dua
endpoint yang didokumentasikan laporan itu tidak ada di backend.

Laporan pendampingnya, `testing/laporan-testing-admisi-penempatan-episode.md`, **tidak** bermasalah
pada titik yang sama: kelima endpoint yang didokumentasikannya terbukti ada di source. Klaim
"100% SUKSES" pada laporan itu tidak ditelusuri mendalam pada kesempatan ini dan tidak sedang
dipersoalkan di sini.

---

## 2. Daftar temuan

### ISS-EPS-01 — Admisi bayi baru lahir tidak dapat diselesaikan

| | |
| --- | --- |
| **Area** | Frontend (kontrol), Backend (endpoint yang belum ada) |
| **Keparahan** | **High** — turun dari `Blocker` sesudah keputusan pemilik 23 September 2026 |
| **Status bukti** | SUDAH-VERIFIKASI di source |
| **Gejala** | Petugas admisi memilih tipe "Bayi Baru Lahir", lalu tombol "Lanjut ke Pembayaran" tidak pernah bisa aktif |

**Akar masalah.** Panel "Episode Ibu" disuapi daftar pilihan yang dikunci kosong secara permanen —
`inpatient-admission-flow-constants.jsx` baris 179–185:

```javascript
export const EMPTY_MOTHER_EPISODE_SELECT = Object.freeze({
  options: Object.freeze([]),
  loading: false,
  loadingMore: false,
  hasMore: false,
  helperText: "Pilihan episode ibu akan dihubungkan pada task integrasi berikutnya.",
});
```

Nilai itu dipakai apa adanya oleh `inpatient-admission-view.jsx` baris 125, tanpa sumber data.
Sementara penjaga lanjutnya menuntut episode ibu terisi — `use-inpatient-admission-flow.jsx`
baris 256–257:

```javascript
const canContinuePatientType = Boolean(
  patientType && (!isNewborn || motherEpisodeId),
);
```

Karena daftar pilihannya kosong permanen, `motherEpisodeId` tidak akan pernah terisi, sehingga
`canContinuePatientType` selalu `false` untuk tipe bayi baru lahir. Alurnya buntu.

**Penyebab sebenarnya ada di backend: tidak ada endpoint yang mendaftar episode ibu aktif.**
`InpatientEpisodeController` hanya memiliki `GET filters/metadata`, `GET summary`, `GET {id}`,
`GET {id}/doctor-assignments`, `GET {id}/nurse-assignments`, dan `GET {id}/status-history`. Tidak
ada `active-mothers`, dan tidak ada daftar episode umum pada controller itu.

**Yang sudah siap dan tidak perlu dibuat ulang.** Sisi data backend sudah selesai lewat
`BE-RWI-031` (✅): kolom `InpEpisode.MotherEpisodeId` ada (baris 44), boks bayi `MstBed.IsForNewborn`
ada, census sudah menampilkan ibu dan bayi sebagai dua baris, dan `FE-RWI-027` sudah mempertahankan
`motherEpisodeId` saat `PUT /episodes/{id}`. Yang hilang **hanya** jalan untuk memilihnya.

**Dampak nyata.** Rumah sakit tidak dapat mendaftarkan bayi baru lahir lewat alur admisi. Petugas
admisi akan memilih kartu itu — kartu itu terlihat sama layaknya lima kartu lain — lalu mentok tanpa
penjelasan yang berarti. Satu-satunya petunjuk di layar adalah kalimat teknis *"Koneksi data episode
ibu berada di luar scope FE-RWI-022"*, yang tidak berarti apa pun bagi petugas.

**Keputusan pemilik — 23 September 2026.** Admisi bayi baru lahir **belum masuk lingkup rilis ini**.
Diputuskan Muhammad Hamzah.

Konsekuensinya, yang dikerjakan sekarang **bukan** menyambungkan episode ibu, melainkan menutup
jalan buntunya supaya petugas admisi tidak terjebak. Pembuatan endpoint episode ibu aktif beserta
penyambungan pilihannya ditunda sampai rilis yang memuat kemampuan itu.

Keparahan butir ini karena itu **turun dari `Blocker` menjadi `High`**: yang tersisa bukan lagi alur
yang buntu diam-diam, melainkan kebutuhan agar keadaan "belum tersedia" itu dinyatakan jujur di
layar.

**Usulan bila kelak diputuskan dikerjakan.**

Backend menyediakan daftar episode ibu aktif. Dua pilihan:

1. *Endpoint khusus* `GET /episodes/active-mothers` — kontraknya jelas dan aturan penyaringannya
   tinggal di satu tempat. **Rekomendasi.**
2. *Memakai census yang sudah ada* — `InpatientCensusController` punya `GET /` yang mendaftar episode
   aktif. Menghemat endpoint baru, tetapi penyaringan "layak menjadi ibu" berpindah ke frontend, dan
   itu menaruh aturan klinis di tempat yang salah.

Aturan penyaringan yang wajib ditegakkan backend, diturunkan dari acceptance criteria `BE-RWI-031`
butir 5: `MotherEpisodeId` **tidak boleh** menunjuk episode milik pasien yang sama. Episode yang
sudah tertutup juga tidak layak dipilih.

**Acceptance criteria.**

1. Petugas dapat memilih episode ibu yang sedang dirawat, dicari dengan nama atau nomor episode.
2. Sesudah episode ibu dipilih, tombol "Lanjut ke Pembayaran" aktif dan alur admisi bayi selesai sampai penempatan.
3. Episode bayi yang terbentuk menyimpan `MotherEpisodeId` yang benar, dan census menampilkan ibu beserta bayinya sebagai dua baris.
4. Episode milik pasien yang sama tidak muncul sebagai calon episode ibu.
5. Alert teknis *"di luar scope FE-RWI-022"* dihapus dari layar.

---

### ISS-EPS-02 — Laporan mendokumentasikan dua endpoint yang tidak ada

| | |
| --- | --- |
| **Area** | Dokumentasi pengujian |
| **Keparahan** | High — merusak kepercayaan pada dokumen |
| **Status bukti** | SUDAH-VERIFIKASI di source |

Bagian 5 laporan menyajikan dua endpoint bergaya Swagger:

| Endpoint yang didokumentasikan | Keadaan sebenarnya |
| --- | --- |
| `GET /api/v1/health-services/inpatient-management/episodes/patient-types` | **Tidak ada.** Nol hasil di seluruh `Areas/` |
| `GET /api/v1/health-services/inpatient-management/episodes/active-mothers` | **Tidak ada.** Nol hasil di seluruh `Areas/` |

Untuk yang pertama, laporan bahkan mencantumkan badan respons `200 OK` lengkap berisi enam objek
beserta field `requiresMotherEpisode`. Respons itu tidak pernah diterima dari mana pun.

Daftar enam tipe pasien sebenarnya **hardcoded di frontend**, pada
`inpatient-admission-flow-constants.jsx` baris 131–138 (`INPATIENT_PATIENT_TYPE`) beserta
`INPATIENT_PATIENT_TYPE_OPTIONS` di bawahnya. Alur Langkah 3 yang diuji tidak memanggil backend sama
sekali untuk keperluan ini.

**Perbaikan yang diusulkan.** Hapus atau koreksi Bagian 5 laporan itu, dan nyatakan apa adanya bahwa
daftar tipe pasien berasal dari konstanta frontend. Bila kelak daftar itu memang dipindahkan ke
backend, itu perubahan kontrak tersendiri yang punya task-nya sendiri.

---

### ISS-EPS-03 — Klaim "100% SUKSES (ALL 6 TYPES PASSED)" tidak didukung isi laporan

| | |
| --- | --- |
| **Area** | Dokumentasi pengujian |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI pada dokumen itu sendiri |

Matriks pada Bagian 3 menandai tipe **Bayi Baru Lahir** dengan `✅ PASS`, sementara kolom Navigasi
Pembayaran pada baris yang sama berisi `N/A (Tertahan)`. Alur yang tidak dapat diselesaikan tetap
dihitung sebagai lulus, lalu dijumlahkan menjadi 6/6.

Bagian 4 menyebut keadaan buntu itu sebagai *"Validasi Penjagaan (Guard Rule)"* dan Bagian 6
menyimpulkannya sebagai *"Validasi Bisnis Kokoh"* yang *"berhasil membuktikan bahwa sistem mencegah
kesalahan operasional staf admisi"*. Tombol yang tidak pernah bisa aktif bukan penjagaan yang
bekerja — itu fitur yang belum tersambung.

Laporan itu **mengutip buktinya sendiri** pada Bagian 4: alert *"Koneksi data episode ibu berada di
luar scope FE-RWI-022. Kontrol ditampilkan sekarang agar aturan bayi baru lahir tidak hilang dari
kerangka."* Kalimat itu menyatakan fiturnya sengaja belum tersambung, lalu artinya diabaikan.

**Perbaikan yang diusulkan.** Turunkan status laporan menjadi 5 dari 6 tipe lulus, dan tandai Bayi
Baru Lahir sebagai `BLOCKED` beserta sebabnya.

---

### ISS-EPS-04 — `FE-RWI-022` berstatus ✅ padahal kemampuannya belum tersedia

| | |
| --- | --- |
| **Area** | Roadmap dan traceability |
| **Keparahan** | Medium |
| **Status bukti** | SUDAH-VERIFIKASI di roadmap |

`roadmap/frontend-roadmap.md` menandai `FE-RWI-022` ✅ **SELESAI**, dan `F9 — Alur admisi` disebut
*"selesai 1 September 2026. Keenam task terimplementasi penuh"*.

Acceptance criteria butir 5 task itu berbunyi: *"Jenis pasien bayi baru lahir menampilkan pilihan
episode ibu; jenis lain tidak."* Dibaca harfiah, kriteria itu terpenuhi — panelnya memang tampil.
Dibaca menurut maksudnya, tidak: `requirement-traceability.md` baris 232 memetakan
`EPIC RI-33` ke *"`FE-RWI-022` untuk memilih episode ibu"*, dan **memilih** itulah yang tidak dapat
dilakukan.

**Perbaikan yang diusulkan.** Turunkan `FE-RWI-022` menjadi 🟡 dengan menyebut butir 5 belum
terpenuhi pada tingkat kemampuan, atau biarkan ✅ dan catat kekurangannya secara eksplisit pada
`EPIC RI-33`. Yang tidak boleh adalah membiarkan keduanya menyatakan selesai tanpa catatan.

---

### ISS-EPS-05 — Penghitung nomor task pada roadmap sub-modul ini sudah basi

| | |
| --- | --- |
| **Area** | Roadmap |
| **Keparahan** | Low |
| **Status bukti** | SUDAH-VERIFIKASI 23 September 2026 |

`backend-roadmap-v2.md` mencatat `task_id_next_free: BE-RWI-127` dan `frontend-roadmap-v2.md`
mencatat `FE-RWI-095`. Kedua nomor itu **sudah dipakai** sub-modul `dokter-rawat-inap` pada
23 September 2026.

Deret `BE-RWI-###` dan `FE-RWI-###` berjalan lurus melintasi seluruh sub-modul dan nomornya tidak
pernah dipakai ulang, sedangkan penghitungnya disalin di beberapa berkas roadmap sekaligus. Siapa pun
yang membuka berkas ini lebih dulu akan mengambil nomor yang sudah terpakai.

Deret bebas sebenarnya saat issue ini ditulis: **`BE-RWI-128`** dan **`FE-RWI-096`**.

**Perbaikan yang diusulkan.** Perbarui kedua penghitung di sub-modul ini. Untuk jangka panjang,
pertimbangkan satu berkas penghitung tunggal di tingkat modul agar tidak ada lagi dua sumber
kebenaran.

---

## 3. Urutan yang disarankan

1. ~~Putuskan apakah admisi bayi baru lahir masuk rilis ini~~ — **sudah dijawab 23 September 2026:
   belum masuk rilis ini.**
2. `ISS-EPS-05` — perbaiki penghitung nomor task sebelum task baru dialokasikan, agar tidak bentrok.
3. `ISS-EPS-01` versi rilis ini — tutup jalan buntunya di layar lewat `FE-RWI-096`, tanpa menyentuh
   backend.
4. `ISS-EPS-02` dan `ISS-EPS-03` — koreksi laporan pengujian. Murni dokumentasi, dapat berjalan
   paralel, dan tidak menunggu apa pun.
5. `ISS-EPS-04` — turunkan `FE-RWI-022` menjadi 🟡 beserta catatan bahwa butir 5 baru terpenuhi
   secara tampilan, bukan kemampuan.
6. **Ditunda ke rilis berikutnya**: `BE-RWI-128` untuk endpoint episode ibu aktif, lalu penyambungan
   pilihannya di frontend.

---

## 4. Catatan pola

Temuan `ISS-EPS-02` dan `ISS-EPS-03` adalah pola yang sama dengan `ISSUE-DOK-001` pada sub-modul
`dokter-rawat-inap`: laporan menyatakan lulus sempurna sambil memuat bukti yang membantahnya di
dokumen yang sama, dan mendokumentasikan respons API yang tidak pernah benar-benar diterima.

Prompt penguji yang sudah disiapkan di
[`../../dokter-rawat-inap/testing/prompt-testing-issue-001.md`](../../dokter-rawat-inap/testing/prompt-testing-issue-001.md)
memuat empat aturan pelaporan yang menutup pola ini. Aturan yang sama layak dipakai untuk pengujian
sub-modul `episode-rawat-inap` berikutnya.
