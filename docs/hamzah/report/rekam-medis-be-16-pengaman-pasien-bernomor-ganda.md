# Rekam Medis — `BE-16` Pengaman pasien bernomor rekam medis ganda

| | |
|---|---|
| Tanggal | 2026-08-27 |
| Task ID | `BE-16` — roadmap `docs/module-blueprints/rekam-medis/roadmap/backend-roadmap.md` |
| Branch | `yoga` (repository backend, tidak ada operasi Git write) |
| Trace | `RM-CAP-007`; `RM-DEC-026`; validation matrix bagian 4; api-contract bagian 2 kode `409` |
| Verifikasi | `AT-RM-22` |
| Migration | **Tidak ada** |
| Endpoint baru | **Tidak ada** |
| Bukti | `dotnet test` → `Failed: 0, Passed: 123`. 7 uji baru, seluruhnya lulus |
| Breaking change | **Tidak** — perilakunya sudah berjalan sejak `BE-11`; yang berubah hanya nomor pengganti yang disebut menjadi lebih tepat |

---

## 1. Masalah yang diselesaikan

Seorang pasien dapat memiliki dua nomor rekam medis, misalnya karena terdaftar dua kali. Sistem
menyediakan kolom `MergedToPatientId` untuk menandai bahwa nomor lama sudah digabungkan ke
nomor baru.

**Masalahnya: penggabungan itu hanya penandaan.** Penelusuran terarah 24 Agustus 2026
(`RM-CAP-007`) menemukan bahwa menyetel `MergedToPatientId` menuliskan satu penunjuk pada baris
pasien dan **tidak melakukan apa pun** terhadap riwayat klinisnya:

| Yang dicari | Hasil |
|---|---|
| Perpindahan data klinis | **Nol** kemunculan `MergedToPatientId` di `ClinicalManagement`, `RegistrationManagement`, maupun `PharmacyManagement` |
| Query yang mengikuti penunjuk itu | **Tidak ada satu pun** di seluruh sistem |
| Penetapan `PatientStatus.Merged` | Tidak ada kode yang menetapkannya; nilainya hanya dipakai sebagai label tampilan |

Akibatnya pasti, bukan kemungkinan: **riwayat pasien bernomor ganda selalu tampil terpecah.**
Bukan karena ada yang keliru, melainkan karena memang tidak ada yang menyatukannya.

Tanpa pengaman, layar rekam medis akan menampilkan sebagian riwayat sebagai riwayat lengkap.
Itu kekeliruan yang paling berbahaya pada berkas rekam medis — pembacanya tidak punya cara tahu
bahwa ada bagian yang hilang.

## 2. Keputusan yang dijalankan

Closure question nomor 8 menawarkan tiga pilihan: menolak membuka berkasnya, menyatukan saat
dibaca, atau memindahkan data klinis sungguhan.

`RM-DEC-026` memilih **yang pertama**, disahkan 26 Agustus 2026:

> Berkas pasien yang ditandai digabung ditolak dengan kode `409` disertai nomor rekam medis
> pengganti, bukan ditampilkan riwayat sebagiannya.

Alasannya: menolak itu jujur. Dua pilihan lain menjanjikan penyatuan yang belum tentu benar —
menyatukan saat dibaca menyembunyikan bahwa datanya memang terpisah, sedangkan memindahkan data
klinis adalah pekerjaan besar yang menyentuh tabel yang sedang dipakai IGD, antrean dokter, dan
farmasi.

`RM-DEC-026` juga menurunkan prioritas task ini menjadi **pengaman**, bukan kebutuhan mendesak.

## 3. Yang sudah ada sejak `BE-11`

Sebagian besar perilakunya sudah berjalan. Pemeriksaan `MergedToPatientId` diletakkan di
`MedicalRecordAccessAuditService`, yang dipanggil **setiap** endpoint berkas rekam medis
sebelum isi diambil.

Karena itu `BE-16` sebagian besar berupa **pembuktian**, bukan penulisan perilaku baru. Ini
hasil langsung dari keputusan `BE-11` meletakkan aturan kewenangan di satu service: pengaman
baru otomatis berlaku pada seluruh endpoint, termasuk endpoint yang belum ada saat aturannya
ditulis.

**Catatan letak.** Roadmap menyebut scope "pemeriksaan pada `MedicalRecordController`".
Kenyataannya pemeriksaan berada di service yang dipanggil controller. Diletakkan di sana agar
aturannya ditulis satu kali, bukan empat kali di empat endpoint — dan empat penulisan berarti
empat kesempatan untuk lupa.

## 4. Yang benar-benar ditambahkan: penelusuran rantai penggabungan

Sebelumnya nomor pengganti diambil **satu langkah** saja:

```
A digabung ke B  →  pesan menyebut nomor B
```

Itu keliru bila B ikut digabungkan:

```
A digabung ke B, B digabung ke C  →  pesan menyebut nomor B
                                     → pengguna membuka B → ditolak lagi
```

Petunjuk menyesatkan seperti itu lebih buruk daripada tidak ada petunjuk, karena pengguna akan
menyangka sistemnya rusak. Sekarang rantainya ditelusuri sampai ujung, dan yang disebut adalah
nomor yang **benar-benar dapat dibuka**.

### Kenapa rantai mungkin terjadi

Pemeriksaan saat menggabungkan (`PatientController.cs:2358-2395`) hanya memastikan pasien tujuan
**ada dan aktif**. Tidak ada pemeriksaan apakah tujuan itu kelak ikut digabungkan. Jadi rantai
terbentuk tanpa ada aturan yang dilanggar.

### Dua pengaman pada penelusuran

| Pengaman | Nilai | Mencegah |
|---|---|---|
| Batas langkah | 10 | Rantai sangat panjang membebani satu permintaan |
| Catatan pasien yang sudah dilewati | — | Rantai melingkar (A ke B, B kembali ke A) berjalan tanpa akhir |

Bila salah satu pengaman menyala, nomor terakhir yang sempat ditemukan tetap dikembalikan. Itu
masih lebih berguna daripada tidak memberi nomor sama sekali.

## 5. Daftar berkas

| Berkas | Status | Keterangan |
|---|---|---|
| `Areas/.../MedicalRecordManagement/Services/MedicalRecordAccessAuditService.cs` | Diperbarui | `CariNomorPenggantiAsync` menggantikan pengambilan satu langkah |
| `tests/.../MergedPatientGuardTests.cs` | Baru | 7 uji |

Tidak ada tabel baru, tidak ada migration, tidak ada endpoint baru, tidak ada perubahan kontrak.

## 6. Verifikasi

```powershell
dotnet test tests\QuilvianSystemBackend.Tests\QuilvianSystemBackend.Tests.csproj
```

| Hasil | Angka |
|---|---|
| Kompilasi | **0 error**, tanpa warning dari berkas modul rekam medis |
| Uji seluruh suite | **Failed: 0, Passed: 123, Skipped: 0** — naik dari 116 |
| Uji `BE-16` | 7 uji, seluruhnya lulus |
| Durasi | 1 menit 49 detik |

| Acceptance criteria | Uji |
|---|---|
| 1) `409` disertai nomor pengganti, pada **seluruh** pintu masuk (`AT-RM-22`) | `PasienHasilPenggabungan_DitolakPadaSeluruhPintuMasukBerkas` |
| 2) Riwayat sebagian tidak ditampilkan walaupun datanya ada | `RiwayatSebagian_TidakDitampilkanWalaupunDatanyaAda` |
| Penolakan `409` tidak menghasilkan jejak akses | `Penolakan409_TidakMenghasilkanJejakAkses` |
| Rantai penggabungan menyebut nomor ujung rantai | `PenggabunganBerantai_MenyebutNomorUjungRantai` |
| Rantai melingkar tetap dijawab, tidak menggantung | `RantaiPenggabunganMelingkar_TetapDijawabTanpaMenggantung` |
| Nomor pengganti selalu menunjuk pasien yang ada | `NomorPengganti_SelaluMenunjukPasienYangAda` |
| Pasien tujuan penggabungan tetap dapat dibuka | `PasienTujuanPenggabungan_TetapDapatDibuka` |

### Tiga uji yang patut disorot

**Keempat pintu masuk diperiksa, bukan hanya riwayat.** Ringkasan, riwayat, detail dokumen, dan
catatan pribadi diuji satu per satu dalam satu uji. Satu pintu yang lupa dijaga sudah cukup
untuk menampilkan riwayat terpecah, dan pengaman yang berlubang di satu tempat bukan pengaman.

**Pasien tujuan penggabungan tetap dapat dibuka.** Diuji tersendiri karena kekeliruan ke arah
sebaliknya berakibat fatal: bila nomor penggantinya ikut tertutup, pasiennya justru kehilangan
seluruh berkas — kebalikan dari maksud `RM-DEC-026`.

**Nomor pengganti selalu menunjuk pasien yang ada.** `MergedToPatientId` memiliki foreign key
sungguhan, sehingga menunjuk pasien yang tidak ada ditolak basis data. Uji ini mencatat jaminan
itu, dan menjaga agar cabang "nomor pengganti tidak diketahui" pada service tidak dibuang
seseorang dengan anggapan mubazir — cabang itu pengaman terakhir, bukan jalur yang dapat
dicapai pemakaian normal.

## 7. Yang belum diverifikasi

| Hal | Alasan |
|---|---|
| Lapisan HTTP dan hak akses | Uji memanggil controller langsung, sesuai keterangan pada `ControllerTestHarness` |
| Perilaku pada data nyata | Belum ada pemeriksaan pada salinan data sungguhan — lihat bagian 8 |

## 8. Risiko yang perlu diketahui sebelum modul dipakai

**Closure question nomor 10 masih terbuka:** berapa banyak pasien yang `MergedToPatientId`-nya
sudah terisi pada data nyata. Pertanyaan itu tidak dapat dijawab dari source; perlu pemeriksaan
data.

Akibatnya perlu dinyatakan terus terang: **bila ternyata ada pasien seperti itu, berkas mereka
menjadi tidak dapat dibuka lewat nomor lama sejak modul ini dipakai.** Pengguna wajib membuka
nomor penggantinya. Itu memang perilaku yang dikehendaki `RM-DEC-026` — tetapi jumlahnya perlu
diketahui lebih dulu agar unit rekam medis tidak terkejut.

Satu hal yang meringankan, dicatat pada `RM-FACT-008`: fitur penggabungan **tidak dapat dipakai
dari antarmuka** karena layar tidak pernah mengirim `mergeReason`, sementara backend
mewajibkannya. Selama celah itu terbuka, tidak ada pasien bernomor ganda baru yang tercipta.

Pemeriksaan yang disarankan sebelum modul dipakai:

```sql
SELECT COUNT(*) FROM public."MstPatient"
WHERE "MergedToPatientId" IS NOT NULL AND "IsDelete" = false;
```

Bila hasilnya nol, `BE-16` benar-benar hanya pengaman. Bila tidak nol, daftar nomornya perlu
diserahkan ke unit rekam medis sebelum modul dipakai.

## 9. Status Git

Tidak ada operasi Git write. Tidak ada `add`, `commit`, `push`, `pull`, `merge`, maupun `rebase`.

Perubahan pengguna yang tidak terkait dengan task ini tidak disentuh.

## 10. Task berikutnya

**Milestone B3 tuntas.** Sisa modul tinggal milestone B4:

| Task | Isi |
|---|---|
| `BE-17` | Uji jalur gagal lengkap |
| `BE-18` | Swagger dan catatan rilis |

Di luar kode, tiga butir masih menahan penyelesaian penuh modul: isi awal master keperluan akses
(`BE-09`), penjalanan pengisian data lama (`BE-08`), dan pemberitahuan penulis CPPT (`BE-15`).
