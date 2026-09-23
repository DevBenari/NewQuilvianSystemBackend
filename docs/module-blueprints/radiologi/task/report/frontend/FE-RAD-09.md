# Laporan Perubahan Frontend — `FE-RAD-09`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RAD-09` |
| Judul | Pengambilan citra, mutu, dan bahan terpakai |
| Epic | `EPIC RAD-05` |
| Requirement | `FR-RAD-041` (penomoran blueprint) |
| Roadmap | `roadmap/frontend-roadmap.md` bagian 4, gelombang `MVP-3` |
| Contract version | `RAD-API-001` grup *Rad Study* (`v1`, berjalan); `RAD-STATE-001` bagian 2 |
| Acceptance criteria | `AC-41` |
| Test yang diminta roadmap | Penanda "Pengulangan dari study ke-1" beserta sebabnya terlihat di daftar study |
| **Ketentuan mengikat** | `RAD-ARCH-FE-001` bagian 5 butir 7 — penanda pengulangan wajib terlihat |
| Dependency | `FE-RAD-08` **selesai** |
| Task mode | `FRONTEND` — frontend target tulis; backend strict read-only kecuali laporan ini dan baris status roadmap |
| Model | Claude Opus 5 (`claude-opus-5`) |
| Branch frontend | `YogaV2`, upstream `origin/YogaV2` |
| Tanggal | 2026-09-14 |
| Status | **Selesai sebagian — lihat bagian 4.1.** 16 unit test baru lulus; 859 unit test repository lulus, 0 gagal. Definition of Done tidak dapat dipenuhi seluruhnya karena empat transisi pada kontrak **tidak punya endpoint** |

---

## 1. Ketentuan mengikat: penanda pengulangan wajib terlihat

`RAD-ARCH-FE-001` bagian 5 butir 7. Dipenuhi `describeRepeatOrigin`, dengan satu aturan yang
sengaja dibuat lebih keras daripada bunyi ketentuannya:

> **Selama `repeatOfStudyId` terisi, penandanya muncul — walaupun nomor urut study asalnya tidak
> dapat disebut.**

Alasannya ada pada risiko task ini: *petugas perlu tahu pasien sudah pernah disinari
sebelumnya*. Nomor urut hanya melengkapi kalimatnya. Kalau penanda disembunyikan ketika study
asal tidak ketemu di daftar saudaranya, yang hilang bukan nomornya — yang hilang adalah fakta
bahwa pasien ini sudah pernah kena radiasi. `S2` mengunci perilaku itu, termasuk untuk daftar
saudara yang kosong dan yang `null`.

**Nomor urutnya harus dicari sendiri.** `RadStudyResponse` membawa `repeatOfStudyId` berupa
`Guid`, **bukan** `studySequence` milik study asalnya. Kalimat "Pengulangan dari study ke-1"
karena itu hanya dapat disusun dengan mencocokkan id itu ke saudara-saudaranya pada pesanan yang
sama — dan seluruh study satu pesanan memang sudah terbawa satu balasan `GET /rad-orders/{id}`,
sehingga pencocokannya tidak menambah permintaan apa pun.

Penandanya muncul di **dua** tempat: spanduk di kepala konsol untuk study yang sedang
dikerjakan, dan lencana pada setiap baris daftar study — yang kedua inilah yang diminta test
roadmap, lengkap dengan sebab dan alasannya.

---

## 2. Satu konsol, bukan layar kedua

Layar ini **tumbuh di route `FE-RAD-08`**, bukan di route baru:
`/health-services/radiology-management/rad-studies/[radOrderId]`.

Alasannya bukan penghematan berkas. Radiografer mengerjakan verifikasi identitas, gerbang
keselamatan, lalu pengambilan citra pada **satu pasien di satu konsol**, tanpa jeda. Memecahnya
menjadi dua halaman berarti memaksa navigasi di tengah pemeriksaan, saat pasien sudah berada di
ruang alat.

Akibat teknisnya sejalan: ketiga langkah bekerja pada study yang sama, dimuat satu permintaan.
Hook `useRadSafetyGate` tetap menjadi satu-satunya pemilik keadaan study dan diperluas dengan
tindakan acquisition; panel langkah 3 murni penyaji dan menerima seluruhnya lewat prop. Tidak
ada pemuatan kedua, dan tidak ada salinan keadaan study di tempat kedua yang cepat atau lambat
berselisih.

---

## 3. Tiga bentuk balasan yang tidak seragam

Ini bagian yang paling mudah salah, dan sumbernya harus dibaca untuk mengetahuinya:

| Endpoint | Balasan | Akibatnya bagi layar |
| --- | --- | --- |
| `start`, `complete`, `abort`, `repeat` | `RadStudyResponse` | Dipakai apa adanya sebagai keadaan study |
| `decide-quality` | **`RadStudyActionResult`** — `record(Study, Handoff)` | Study diambil dari ruas `study`; membungkusnya utuh akan menaruh objek yang salah sebagai study |
| `consumptions` | **`RadConsumptionResponse`** — bukan study sama sekali | Keadaan study **tidak** ditimpa; rincian pesanan disegarkan |

`jalankan` karena itu menerima `ambilStudy` dan `segarkan`. Memperlakukan ketiganya sama akan
membuat layar menampilkan keadaan yang bukan study setelah menilai mutu atau mencatat kontras.

`repeat` juga halus: balasannya adalah study **baru**, bukan study asal. Study baru itulah yang
menjadi study aktif, dan rincian pesanan ikut disegarkan supaya study asal tetap terbaca di
daftar saudaranya — ia memang tidak pernah ditimpa.

---

## 4. Dua temuan

### 4.1 Empat transisi pada `RAD-STATE-001` bagian 2 tidak punya endpoint

`RadStudyController` punya **16 endpoint**, dan tidak satu pun di antaranya menjalankan:

| Transisi pada kontrak | Endpoint |
| --- | --- |
| `QualityRejected` → Tandai perlu diulang → `RepeatRequired` | **Tidak ada** |
| Beberapa status → Tahan → `OnHold` | **Tidak ada** |
| `OnHold` → Lanjutkan → status sebelumnya | **Tidak ada** |
| Sebelum `AcquisitionStarted` → Batalkan → `Cancelled` | **Tidak ada** |

Diperiksa langsung: `StudyStatus = RadStudyStatus.RepeatRequired` **tidak pernah ditulis di
mana pun**. Nilai itu hanya dibaca — pada daftar status yang boleh diulang dan pada peta label.
Tiga status karenanya tidak dapat dicapai sama sekali lewat API: `RepeatRequired`, `OnHold`
tingkat study, dan `Cancelled` tingkat study.

**Akibatnya bagi Definition of Done.** "Seluruh transisi study dapat dijalankan" **tidak dapat
dipenuhi frontend**, karena empat di antaranya tidak punya jalan. Yang dikerjakan: seluruh
transisi yang **punya** endpoint dijalankan — mulai, selesaikan, hentikan, nilai mutu, ulangi,
catat bahan — dan keempat yang tidak punya dinyatakan apa adanya di layar, tanpa dibuatkan
tombol. Tombol yang tidak melakukan apa pun lebih buruk daripada tombol yang tidak ada:
petugas akan menekannya saat pasien menunggu.

**Perlu keputusan pemilik modul**: menambah endpointnya di backend, atau mencoret keempat
transisi itu dari `RAD-STATE-001` bagian 2.

### 4.2 Metadata penyaring tidak menerbitkan tiga enum yang dibutuhkan layar

`GET /rad-studies/filters/metadata` menerbitkan `StudyStatuses`, `SafetyCheckStates`, dan
`SafetyRuleStatuses` lengkap dengan label Indonesianya. Ia **tidak** menerbitkan `RadAbortCause`,
`RadRepeatCause`, maupun `RadConsumptionItemType` — padahal ketiganya justru yang dipakai isian
pada layar ini.

Ketiganya terpaksa disalin ke `rad-order-constants.jsx`, beserta labelnya. **Ini berarti
perubahan enum di backend tidak akan terlihat di layar sampai berkas konstanta ikut disunting** —
persis jenis perselisihan yang dihindari untuk status study. Penyebabnya dicatat di berkasnya
supaya tidak terbaca sebagai pilihan.

Menambahkan ketiganya ke `RadStudyFilterMetadataResponse` adalah perubahan kecil dan sudah ada
presedennya di berkas yang sama. **Perlu keputusan pemilik modul.**

---

## 5. Penilaian mutu tidak dapat dikirim ganda

Definition of Done. Dipenuhi dua lapis, dan lapis keduanya bukan hiasan:

1. **Penjaga status.** `DecideQualityAsync` hanya menerima `Acquired`. Sekali dinilai, status
   berpindah ke `QualityAccepted` atau `QualityRejected`, dan penilaian kedua ditolak server.
   Layar mengikuti penjaga yang sama lewat `canDecideQuality`.
2. **Penjaga kiriman ganda dalam satu turn.** `sedangMengirimRef` menolak lebih dulu daripada
   `actionLoading`, karena `actionLoading` baru benar setelah React merender ulang — dua klik
   berdekatan masih sempat lolos tanpanya. Polanya menyalin `FE-RAD-05`.

Lapis kedua penting justru di sini: penilaian "layak" **menerbitkan fakta kelayakan tagih ke
Billing**, dan `EmitChargeEligibilityAsync` dijalankan di luar transaksi karena fakta klinis yang
sudah terkirim tidak dapat ditarik oleh rollback. Layar juga menambahkan konfirmasi tersendiri
sebelum mengirim, dan menyatakan dengan jelas ketika `billingFactSubmitted` sudah benar — supaya
petugas tahu penilaian itu sudah berakibat di luar Radiologi.

---

## 6. Perubahan yang dikerjakan

| Berkas | Keadaan |
| --- | --- |
| `src/lib/hooks/health-services/radiology-management/rad-acquisition-rules.js` | Baru — fungsi murni; penanda pengulangan, penjaga transisi, pembentuk keempat muatan |
| `src/components/view/health-services/radiology-management/rad-safety-gate/rad-acquisition-panel.jsx` | Baru — panel langkah 3, murni penyaji |
| `src/style/health-services/radiology-management/rad-safety-gate/rad-acquisition.module.css` | Baru — 26 kelas, seluruhnya design token |
| `tests/unit/rad-acquisition-rules.test.mjs` | Baru — 16 test |
| `src/lib/hooks/health-services/radiology-management/use-rad-safety-gate.jsx` | Diubah — enam tindakan acquisition, `ambilStudy`/`segarkan`, penjaga kiriman ganda |
| `src/lib/hooks/health-services/radiology-management/rad-safety-gate-rules.js` | Diubah — `RAD_STUDY_STATUS` dilengkapi menjadi kesebelas nilai enum |
| `src/components/view/…/rad-safety-gate/rad-safety-gate-view.jsx` | Diubah — merender panel langkah 3 |
| `src/lib/constants/health-services/radiology-management/rad-order-constants.jsx` | Diubah — `RAD_ACQUISITION_COPY`, tiga daftar pilihan enum, dua status yang belum ada di `RAD_STUDY_STATUS_META` |

`store.jsx` **tidak disentuh**, tidak ada potongan Redux baru, tidak ada route baru, tidak ada
base component baru, dan tidak ada entri menu baru.

`RAD_STUDY_STATUS` sebelumnya hanya memuat tujuh dari sebelas nilai enum. Keempat sisanya —
`Aborted`, `QualityRejected`, `RepeatRequired`, `Cancelled` — ditambahkan karena status yang
tidak dikenali layar akan tampil sebagai teks mentah kepada petugas.

---

## 7. Validasi

| Yang dijalankan | Hasil |
| --- | --- |
| `node --test tests/unit/rad-acquisition-rules.test.mjs` | **16 lulus, 0 gagal** |
| `node --test tests/unit/` (seluruh repository) | **859 lulus, 0 gagal** |
| `npx eslint` atas seluruh berkas yang disentuh | **0 error, 0 warning** |
| `npm run build` | **Compiled successfully in 34.9s.** Route tetap `/health-services/radiology-management/rad-studies/[radOrderId]` (dynamic) — tidak ada route baru |

Cakupan test: penanda pengulangan pada `S1`–`S3` (termasuk test yang diminta roadmap pada `S1`);
jumlah penyinaran pada `S4`; `isUsable` kosong ≠ tidak layak pada `S5`–`S6`; penjaga kiriman
ganda pada `S7`; urutan transisi pada `S9`–`S12`; keempat muatan pada `S13`–`S15`.

**`MANUAL TEST: NOT FEASIBLE`.** Menjalankan layar ini menuntut study yang sudah berstatus
`SafetyCleared`. Status itu **tidak dapat dicapai di lingkungan mana pun saat ini**: gerbang
keselamatan bersifat fail-closed dan belum ada satu pun aturan keselamatan berstatus `Active`,
karena `RAD-OPEN-011` masih terbuka. Sesi petugas yang memegang `RadStudy : Acquire`,
`RadStudy : Quality`, `RadStudy : Repeat`, dan `RadStudy : Consumption` juga tidak dapat dibuat
dari sini. Seluruh jalur karena itu dikunci unit test terhadap bentuk balasan yang diverifikasi
langsung dari DTO dan service backend, bukan ditebak.

---

## 8. Yang sengaja **tidak** dikerjakan

| Butir | Alasan |
| --- | --- |
| Tombol Tahan, Lanjutkan, Batalkan, dan Tandai Perlu Diulang | Tidak ada endpointnya. Lihat bagian 4.1 |
| Menampilkan ringkasan penyerahan ke Billing (`Handoff`) | Yang perlu diketahui layar — sudah terserah atau belum — sudah terbaca dari `billingFactSubmitted` pada study. Menampilkan isi `ClinicalFactEmissionResult` berarti membocorkan urusan Billing ke layar radiografer |
| Mengubah atau menghapus baris bahan terpakai | Backend hanya menyediakan penambahan. Tidak ada endpoint ubah maupun hapus |
| Route kedua untuk acquisition | Lihat bagian 2 |
| Base component baru | Dilarang `AGENTS.md`. Panel disusun dari `BaseButton`, `ConfirmModal`, `BaseNativeSelectField`, `BaseTextField`, `BaseTextAreaField`, `BaseCheckboxField`, `StatusBadge`, dan `InformationAlert` yang sudah ada |

---

## 9. Risiko dan dependency yang masih terbuka

| Hal | Keadaan |
| --- | --- |
| **Empat transisi tanpa endpoint** | Temuan bagian 4.1. Definition of Done tertahan karenanya. **Perlu keputusan pemilik modul** |
| **Tiga enum tidak terbit pada metadata** | Temuan bagian 4.2. Layar menyalinnya dan akan tertinggal bila enum backend berubah. **Perlu keputusan pemilik modul** |
| **Tidak ada aturan keselamatan `Active`** | `RAD-OPEN-011` terbuka. Selama belum ada, tidak satu pun study mencapai `SafetyCleared`, sehingga **seluruh layar ini belum dapat dijalankan siapa pun** |
| **Study terkunci saat aturan berubah** | Temuan `FE-RAD-08` bagian 4.2, masih terbuka |
| **Kontrak `409` vs jalur API `422`** | Temuan `FE-RAD-08` bagian 4.1, masih terbuka |
| **`HoldAsync` menerima status terminal** | Temuan `FE-RAD-06`, masih terbuka |
| **Daftar kerja tanpa identitas pasien** | Temuan `FE-RAD-07`, masih terbuka |

---

## 10. Riwayat Revisi

| Revision | Tanggal | Perubahan | Status |
|---:|---|---|---|
| 1 | 2026-09-14 | Laporan dibuat. 4 berkas baru, 4 diubah. 16 test baru lulus; 859 test repository lulus; lint 0 error; build lulus. Ketentuan mengikat bagian 5 butir 7 dipenuhi dengan penanda yang tidak pernah disembunyikan. Dua temuan dicatat: empat transisi `RAD-STATE-001` tanpa endpoint — sehingga Definition of Done tertahan — dan tiga enum yang tidak terbit pada metadata penyaring. | `draft` |
