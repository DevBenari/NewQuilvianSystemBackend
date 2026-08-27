# FE-IGD-012â€“018 â€” Penyelesaian Antarmuka IGD

## Metadata

| Field | Nilai |
| --- | --- |
| Task | `FE-IGD-012` sampai `FE-IGD-018` |
| Tanggal | 26 Agustus 2026 |
| Status | Implementasi selesai di working tree; verifikasi runtime Node masih memerlukan izin host |

## Hasil

| Task | Hasil |
| --- | --- |
| `FE-IGD-012` | Normalisasi error menampilkan `response.data.message`, termasuk penolakan `409` triase |
| `FE-IGD-013` | Assesmen awal dapat dibuat dari layar dengan `encounterId`, dimuat ulang setelah simpan, dan hanya menampilkan field yang memiliki rumah pada DTO Patient Assessment |
| `FE-IGD-014` | Payload encounter baru memakai `EncounterType.Emergency`; alasan override episode ganda tersedia dan dikirim ke Emergency Visit |
| `FE-IGD-015` | Consumer berpindah dari `emergency-transfers` ke `emergency-departures`; nol referensi route lama tersisa |
| `FE-IGD-016` | Keadaan fisik dan dokumen serah terima tampil bersamaan serta memakai aksi endpoint terpisah |
| `FE-IGD-017` | Riwayat kejadian menampilkan waktu terjadi/dicatat, pelaku, efektivitas, penyetuju pembalikan, dan form amend yang menolak waktu masa depan |
| `FE-IGD-018` | `test/test.txt` dihapus; folder route kosong tidak memiliki berkas terlacak; nol consumer ditemukan |

## Verifikasi statis

```text
rg "TRANSFER_STATUS|TRANSFER_URL|transferStatus|emergency-transfers|updateTransferStatus" src
=> nol hasil

git diff --check
=> bersih (hanya peringatan normalisasi LF/CRLF)
```

`npm run lint` belum mengeksekusi ESLint karena sandbox Windows menolak Node melakukan `lstat` pada profil pengguna. Permintaan eksekusi host juga ditolak oleh approval reviewer sebelum proses dibuat. Karena itu laporan ini tidak mengklaim lint/build frontend lulus sampai command tersebut dijalankan pada host yang berizin.

## Tidak dilakukan

- Tidak ada perubahan URL halaman/bookmark; tab tetap berada di layar assessment yang sama.
- Tidak ada deployment atau akses database.
- Bukti screenshot dan reload terhadap backend nyata belum dibuat karena tidak ada sesi runtime/kredensial petugas pada pengerjaan ini.

---

## `FE-IGD-001 K1` gagal, dan itu memang seharusnya

`npm test` awalnya **37 lulus, 1 gagal**. Yang gagal:

```
not ok 10 - FE-IGD-001 K1: payload encounter mengirim Outpatient, bukan Emergency
  Expected values to be strictly equal: 2 !== 1
```

Kegagalan ini **sudah diramalkan** `blueprint-manifest.md` bagian 3.1 ketika `BE-IGD-023`
direncanakan. Asersinya menuntut frontend mengirim `Outpatient`, dengan komentar *"Backend
hanya menerima Outpatient"* — kalimat yang sudah tidak benar sejak `BE-IGD-023` mendarat.

Yang berubah adalah **perilaku yang disengaja**, bukan regresi. Karena itu yang diperbaiki
asersinya, bukan frontend-nya. Test diganti menjadi `FE-IGD-014 K1` yang menuntut kebalikannya,
beserta catatan mengapa arahnya dibalik.

`IGD-DEC-109` tetap membuat `Outpatient` **diterima** backend selama masa transisi. Tetapi
"diterima untuk data lama" berbeda dari "pantas dikirim kunjungan baru": kunjungan IGD yang
mengirim `Outpatient` membuat pasien IGD ikut terhitung pada laporan rawat jalan.

Sesudahnya: **38 lulus, 0 gagal**.

---

# `FE-IGD-019` — Assesmen Awal IGD berisi pemeriksaan yang sebenarnya

Ditambahkan atas permintaan owner: *"di frontend bagian pengkajian, assessment awal ini adalah
form pemeriksaan vitalSign, painAssessment dan trus pemeriksaan assesment awal dan pemeriksaan
pernapasan"*.

## Keadaan sebelumnya

Tab **Assesmen Awal IGD** sudah ada, tetapi isinya hanya **17 field** buatan sendiri: keluhan,
riwayat, alergi, tiga field nyeri, risiko jatuh, dan tiga catatan. **Nol tanda vital. Nol
pemeriksaan pernapasan.** Pengkajian nyeri hanya memuat 3 dari 7 kolom yang disediakan
`TrxPatientAssessment`.

Padahal formulir yang lengkap **sudah ada dan sudah dipakai setiap hari** di skrining antrean
perawat rawat jalan:

| Komponen | Isi |
| --- | --- |
| `nurse-station-management/VitalSignTab` | Suhu, nadi, frekuensi napas, saturasi, sistolik, diastolik, berat, tinggi, BMI, MAP, **oksigen dan kesadaran**, GCS |
| `nurse-station-management/AssessmentTab` | Keluhan, **tujuh kolom nyeri**, riwayat keturunan, alergi, imunisasi, nutrisi, risiko jatuh, catatan keperawatan |

## Yang dikerjakan

Tab IGD kini **memakai kedua komponen itu apa adanya**, bukan menirunya. Satu formulir, empat
kelompok, urutannya mengikuti cara perawat memeriksa pasien: tanda vital → pemeriksaan
pernapasan dan kesadaran → pengkajian awal → pengkajian nyeri.

Bedanya dengan rawat jalan hanya satu, dan letaknya **bukan di layar**: pasien IGD tidak punya
baris antrean, sehingga `queueId` dikirim `null`. Itulah yang dibuka `BE-IGD-026` dan
`BE-IGD-027`.

## Satu salinan aturan yang dihapus

Pembentuk payload `POST /patient-assessments` sebelumnya terkunci di dalam
`use-nurse-station-queue.js` dan **tidak diekspor**, sehingga tab IGD terpaksa menyusun
payload-nya sendiri — dua salinan aturan yang sama untuk satu tabel.

Diekstrak menjadi `utils/health-services/clinical-management/patient-assessment-payload.utils.js`,
dan **kedua pemanggil kini memakainya**. Hook skrining perawat tinggal memetakan item antrean
menjadi `encounterId` dan `queueId`.

`RegistrationManagement` pemiliknya belum ditunjuk; perubahan dikerjakan atas wewenang
`IGD-DEC-107`, dan jalur rawat jalan dibuktikan tidak berubah lewat build serta test.

## Dua cacat yang ditemukan dan diperbaiki

**① Tombol simpan yang tidak pernah mati.** `emergency-assessment-initial-tab` dan
`emergency-assessment-transfer-tab` mengirim prop `disabled` dan `error`, sedangkan
`EmergencyAssessmentFormCard` menerima **`canSubmit`** dan **`saveError`**. Akibatnya
`canSubmit` selalu memakai nilai bawaannya `true`:

| Yang dijanjikan kartu | Yang sebenarnya terjadi |
| --- | --- |
| Tombol mati saat isian belum lengkap, disertai alasannya | Tombol **selalu hidup**, dan `disabledHint` tidak pernah tampil |
| Galat penyimpanan tampil di bawah formulir | Galat **tidak pernah tampil** |

Perawat yang menekan Simpan pada formulir kepergian yang belum lengkap karena itu tidak
mendapat penolakan yang terbaca — permintaannya berangkat, ditolak backend, dan pesannya
hilang. Keduanya diperbaiki.

**② `??` tidak jatuh ke cadangan untuk string kosong.** Ditemukan oleh test yang baru ditulis,
bukan oleh pembacaan kode. Dua layar memberi nama berbeda untuk kolom yang sama — skrining
memakai `complaint`, IGD memakai `chiefComplaint` — dan penggabungannya semula ditulis
`form.complaint ?? form.chiefComplaint`. Karena bentuk kosong yang sebenarnya muncul adalah
**string kosong**, bukan `null`, cabang IGD selalu kalah dan keluhan utamanya hilang.
Normalisasi dipindah ke depan: `toNullableText(form.complaint) ?? toNullableText(form.chiefComplaint)`.

## Verifikasi

```
npm run build   => berhasil, standalone runtime siap
npm run lint    => 0 error
npm test        => 46 lulus, 0 gagal   (sebelumnya 38)
```

Delapan test baru di `tests/unit/patient-assessment-payload.test.mjs` menguji keempat kelompok
yang diminta owner, ditambah dua jebakan yang mudah terulang: nilai kosong wajib menjadi `null`
bukan string kosong, dan `queueId` wajib `null` bukan Guid nol.
