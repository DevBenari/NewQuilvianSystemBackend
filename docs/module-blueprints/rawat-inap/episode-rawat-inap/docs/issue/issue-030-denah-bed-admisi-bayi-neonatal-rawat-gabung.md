# ISSUE-030 — Denah Bed Admisi Bayi Belum Muncul Akibat Ketiadaan Episode Draf Serta Pembatasan Kamar Bayi Neonatal dan Validasi Kamar Ibu Tertutup

```yaml
issue_id: ISSUE-EPS-030
module_id: rawat-inap
submodule: episode-rawat-inap
blueprint_id: RWI-BP-001
layar: "Admisi Rawat Inap — Langkah 4 Penempatan dan Pemesanan Bayi (FE-INP-03 / FE-INP-026)"
sumber_laporan: "Laporan pengguna 21 Oktober 2026: denah bed admisi bayi belum muncul, hanya kamar bayi yang bisa diklik pada kondisi neonatal terpisah, dan kamar ibu tidak bisa dipilih jika status penempatan ibu sudah closed"
tanggal_issue: "2026-10-21"
status: SELESAI
keparahan_tertinggi: Blocker
source_sha_backend: "6406fc76 (MHamzah)"
source_sha_frontend: "381ec25fa (HamzahV2)"
rencana_perbaikan: ../plan-repair/plan-repair-030-denah-bed-admisi-bayi-neonatal-rawat-gabung.md
```

---

## 1. Ringkasan

Laporan ini menganalisis kegagalan operasional kritis (*blocker*) pada modul Admisi Rawat Inap ketika petugas melakukan pendaftaran admisi bayi baru lahir. Pada saat petugas menyelesaikan Langkah 3 ("Perawatan dan DPJP Bayi") dan melanjutkan ke Langkah 4 ("Penempatan dan Pemesanan Bayi"), peta denah visual tempat tidur rumah sakit tidak muncul sama sekali di layar. Petugas justru disambut dengan kotak peringatan merah bertuliskan *"Episode Draft belum dapat disiapkan karena tidak ada konteks kamar aktif. Muat ulang atau hubungi pengelola master bed"* beserta tombol *"Coba Siapkan Denah Lagi"*, sehingga alur pendaftaran admisi bayi terhenti total (*deadlock*).

Berdasarkan penelusuran mendalam terhadap kode sumber frontend dan backend, ditemukan bahwa akar masalahnya berakar pada mekanisme pembuatan draf episode otomatis (*pre-flight draft preparation*) di hook `useInpatientAdmissionBed`. Hook tersebut mewajibkan adanya tempat tidur awal (*contextBed*) dari papan lama untuk menciptakan episode draf sebelum denah diizinkan memuat. Karena pada alur admisi bayi draf episode sengaja belum dibuat di Langkah 3 dan daftar tempat tidur bayi yang tersedia dari server belum terisi atau terisolasi, fungsi penyiapan draf gagal dan mengunci layar.

Selain masalah pemuatan denah, laporan ini mengkaji dan merumuskan aturan bisnis yang ditetapkan oleh pemilik sistem untuk dua mode perawatan bayi:
1. **Mode Neonatal Terpisah (`Neonatal`)**: Denah visual seluruh lantai rumah sakit harus tetap tampil apa adanya (*full floor plan view*), namun **hanya kamar bayi/perinatologi dan tempat tidur khusus bayi (`isForNewborn = true`) yang dapat diklik/dipilih**. Kamar dan bed pasien dewasa/umum lainnya tetap terlihat tetapi dalam keadaan non-aktif (*disabled*) dengan penanda kelayakan yang jelas.
2. **Mode Rawat Gabung dengan Ibu (`RoomingIn`)**: Bayi ditempatkan pada boks bayi di kamar penempatan aktif ibu. **Jika status kamar penempatan ibu sudah berstatus tertutup (*closed* / selesai dirawat / tidak ada penempatan aktif)**, maka kamar dan boks ibu tersebut **tidak boleh dapat dipilih**, dan sistem wajib memberikan peringatan pencegahan sejak Langkah 3 maupun pada denah Langkah 4.

Analisis ini menyajikan rekomendasi arsitektur perbaikan adaptasi denah tanpa prasyarat episode draf buatan (mengadopsi pola teruji dari admisi transfer `ISSUE-EPS-024`) serta penegakan gerbang validasi status kamar ibu tertutup.

---

## 2. Laporan Asli Pelapor

| No. Laporan | Ringkasan Keluhan Pelapor | Lampiran Terkait |
| :---: | :--- | :--- |
| **1** | Pada saat pendaftaran admisi bayi belum bisa muncul bed tersebut, saya ingin anda tampilkan bed tersebut. | Lampiran 2: Screenshot layar `Langkah 4 dari 7 Penempatan dan Pemesanan 57% Selesai` menampilkan pesan galat merah *"Episode Draft belum dapat disiapkan karena tidak ada konteks kamar aktif. Muat ulang atau hubungi pengelola master bed"* dan tombol *"Coba Siapkan Denah Lagi"*. Denah tidak muncul. |
| **2** | Hanya kamar bayi saja ya yang bisa diklik, yang lain tetap bisa tampil juga. Kondisi tersebut jika neonatal terpisah maka akan memilih bed ya. | Lampiran 1: Screenshot layar `Langkah 3: Perawatan dan DPJP Bayi` dengan pilihan aktif *"Neonatal terpisah"* dan centang *"Membutuhkan perawatan neonatal intensif sesuai keputusan klinis"*. |
| **3** | Jika bersama ibu berarti pada kamar ibu tersebut, pastikan jika status ibunya sudah closed kamar maka tidak bisa dipilih. | Narasi pesan pengguna. |

---

## 3. Ringkasan Temuan

| ID Temuan | No. Lap | Judul Temuan | Jenis | Area | Keparahan | Status Bukti | ID Perbaikan |
| :--- | :---: | :--- | :--- | :--- | :---: | :--- | :--- |
| `ISS-EPS-030-01` | 1 | Denah tempat tidur admisi bayi terblokir total akibat kegagalan pembuatan episode draf prematur | `BUG` | Frontend | Blocker | `SUDAH-VERIFIKASI` pada source FE | `FIX-EPS-030-01` |
| `ISS-EPS-030-02` | 2 | Belum ada mekanisme adaptasi selektivitas denah khusus kamar bayi pada mode neonatal terpisah | `DESIGN_CHANGE` / `RULE_VIOLATION` | Frontend | High | `SUDAH-VERIFIKASI` pada source FE/BE | `FIX-EPS-030-02` |
| `ISS-EPS-030-03` | 3 | Ketiadaan validasi guard saat kamar penempatan ibu berstatus closed pada mode rawat gabung | `BUG` / `RULE_VIOLATION` | Frontend & Backend | High | `SUDAH-VERIFIKASI` pada source FE/BE | `FIX-EPS-030-03` |
| `ISS-EPS-030-T1` | — | Kontrak endpoint denah backend mewajibkan `EpisodeId` aktif sehingga menghambat pemuatan denah pra-admisi | `CONTRACT_MISMATCH` | Backend / API | Medium | `SUDAH-VERIFIKASI` pada source BE | `FIX-EPS-030-04` |

---

## 4. Rincian Per Temuan

### ISS-EPS-030-01 — Denah Tempat Tidur Admisi Bayi Terblokir Total Akibat Kegagalan Pembuatan Episode Draf Prematur

| Field | Nilai |
| :--- | :--- |
| **No. laporan** | 1 |
| **Jenis** | `BUG` |
| **Area** | Frontend (`use-inpatient-admission-bed.jsx`, `inpatient-admission-view.jsx`) |
| **Keparahan** | Blocker |
| **Status bukti** | `SUDAH-VERIFIKASI` di source code frontend |
| **Perbaikan** | `FIX-EPS-030-01` |

**Apa yang terjadi.**  
Ketika petugas admisi mengisi pendaftaran bayi baru lahir dan berpindah dari Langkah 3 ("Perawatan dan DPJP Bayi") ke Langkah 4 ("Penempatan dan Pemesanan Bayi"), layar tidak menampilkan denah lantai kamar rumah sakit maupun denah boks bayi. Layar seketika memunculkan kotak pesan merah berbunyi:  
*"Episode Draft belum dapat disiapkan karena tidak ada konteks kamar aktif. Muat ulang atau hubungi pengelola master bed."*  
Di bawah pesan terdapat tombol *"Coba Siapkan Denah Lagi"* yang jika ditekan tetap gagal dan mengulang pesan yang sama. Petugas tidak dapat memilih bed dan tidak dapat melanjutkan ke langkah persetujuan wali.

**Kenapa terjadi.**  
Rantai penyebab teknis tertelusur secara langsung dari kode sumber frontend:
1. Pada `src/components/view/health-services/inpatient-management/inpatient-newborn-admission-steps.jsx:131`, pada Langkah 3 fungsi `submit` dokter bayi sengaja di-override hanya untuk memvalidasi isian tanpa membuat episode di database:  
   `submit: async () => ({ success: newborn.validateCare(doctor.form.doctorId) })`. Akibatnya, saat masuk Langkah 4, `doctor.episodeId` bernilai string kosong `""`.
2. Di `src/components/view/health-services/inpatient-management/inpatient-admission-view.jsx:684-699`, properti `admission` dikirim ke hook `useInpatientAdmissionBed`. Hal ini membuat konstanta `usesAdmissionFloorPlan = Boolean(admission || transfer)` bernilai `true` (`use-inpatient-admission-bed.jsx:123`).
3. Pada `src/lib/hooks/health-services/inpatient-management/use-inpatient-admission-bed.jsx:897-918`:
   ```javascript
   useEffect(() => {
     if (!usesAdmissionFloorPlan || episodeId || !active || !admission?.enabled || isTransferAdmission) return;
     if (legacyBoard.loading) { draftSourceLoadingObserved.current = true; return; }
     if ((!draftSourceLoadingObserved.current && !legacyBoard.availableBeds?.length) || legacyBoard.loadError) return;
     prepareFloorPlanDraft();
   }, [...]);
   ```
   Karena `episodeId` kosong, `useEffect` memicu fungsi `prepareFloorPlanDraft()`.
4. Fungsi `prepareFloorPlanDraft` (`use-inpatient-admission-bed.jsx:854-884`) mencoba mencari bed sembarang dari papan lama sebagai konteks draf:
   ```javascript
   const contextBed = source.availableBeds?.find(
     (bed) => bed?.bedId && bed?.patientClassId && bed?.serviceUnitId,
   );
   return runAdmissionPreparation(contextBed);
   ```
5. Pada `use-inpatient-admission-bed.jsx:797-802`:
   ```javascript
   if (!contextBed) {
     const message =
       "Episode Draft belum dapat disiapkan karena tidak ada konteks kamar aktif. Muat ulang atau hubungi pengelola master bed.";
     setActionError(message);
     return { success: false, message };
   }
   ```
   Ketika `legacyBoard.availableBeds` kosong (misalnya karena belum ada bed bayi yang cocok di query awal atau belum terpilih kamar), `contextBed` bernilai `undefined`. Fungsi langsung gagal, menulis pesan galat ke `actionError`, dan menghentikan seluruh proses.
6. Komponen `InpatientAdmissionBedSelectionStep` (`inpatient-admission-bed-step.jsx:129-138`) melihat `!floorPlan?.data && actionError`, sehingga menyembunyikan komponen denah `<FloorPlanSelection />` dan hanya merender kotak pesan merah serta tombol ulangi.

**Apakah ini menyimpang dari desain?**  
Ya, ini adalah `BUG`. Desain pada `05-skema-tampilan.md` dan cetak biru admisi bayi (`issue-028`) menetapkan bahwa penempatan bayi harus menampilkan denah pemilihan bed secara visual tanpa memaksa petugas membuat transaksi draf episode di latar belakang dengan tempat tidur sembarang (*dummy bed context*).

**Dampak nyata.**  
Proses pendaftaran admisi rawat inap untuk seluruh bayi baru lahir di rumah sakit macet total pada Langkah 4. Bidan dan petugas admisi tidak bisa menempatkan bayi, tidak bisa mencetak gelang identitas bayi, dan tidak bisa menerbitkan dokumen admisi.

**Rekomendasi.**  
Ubah perilaku pemuatan denah pada admisi bayi: perlakukan alur admisi bayi (`newbornContext`) seperti alur transfer (`isTransferAdmission`), yaitu denah master data visual dimuat langsung menggunakan endpoint denah layout umum (`purpose: ""`) tanpa menuntut draf episode terlebih dahulu. Pembuatan draf episode dilakukan secara atomik setelah petugas benar-benar memilih tempat tidur yang diinginkan.

---

### ISS-EPS-030-02 — Belum Ada Mekanisme Adaptasi Selektivitas Denah Khusus Kamar Bayi pada Mode Neonatal Terpisah

| Field | Nilai |
| :--- | :--- |
| **No. laporan** | 2 |
| **Jenis** | `DESIGN_CHANGE` / `RULE_VIOLATION` |
| **Area** | Frontend (`use-inpatient-admission-bed.jsx`, `inpatient-admission-bed-step.jsx`) |
| **Keparahan** | High |
| **Status bukti** | `SUDAH-VERIFIKASI` di source code |
| **Perbaikan** | `FIX-EPS-030-02` |

**Apa yang terjadi.**  
Pengguna menetapkan aturan bisnis yang tegas:  
*"Hanya kamar bayi saja ya , yang bisa di klik , yang lain tetap bisa tampil juga. Kondisi tersebut jika neonatal terpisah maka akan memilih bed ya."*  
Saat ini, logika adaptasi denah (`adaptedFloorPlanData`) hanya diterapkan untuk alur transfer (`isTransferAdmission`). Pada alur non-transfer, denah mengandalkan respons server `GET /bed-occupancies/floor-plan?purpose=Admission&episodeId=...`. Bila dipaksa memuat layout umum, seluruh kamar dewasa/umum dan kamar bayi akan memiliki tampilan yang sama tanpa filter selektivitas khusus bayi, sehingga petugas bisa salah mengklik kamar rawat inap dewasa biasa.

**Kenapa terjadi.**  
1. Pada `use-inpatient-admission-bed.jsx:135-212`, useMemo `adaptedFloorPlanData` hanya memeriksa kondisi `if (!isTransferAdmission || !floorPlan.data) return floorPlan.data;`. Belum ada penanganan adaptasi untuk kondisi `isNewbornAdmission`.
2. Di master data rumah sakit, kamar dan tempat tidur memiliki atribut boolean `IsForNewborn` (`MstRoom.IsForNewborn` dan `MstBed.IsForNewborn`, `MstBed.cs:29`, `MstRoom.cs:39`). Kamar perinatologi/NICU memiliki `IsForNewborn = true` atau kelas pasien bertipe Perinatologi (`FloorPlanRoomNaming.cs:167-180`).
3. Tanpa lapisan adaptor selektivitas bayi:
   - Kamar-kamar non-bayi (seperti Kamar Mawar Dewasa, Ruang Bedah, Kamar Isolasi Dewasa) tidak disaring status kliknya.
   - Atau sebaliknya, jika disaring di level query master data dengan `isForNewborn=true`, denah lantai akan bolong (hanya muncul 1 kamar bayi, ruangan lain hilang), melanggar permintaan pengguna: *"yang lain tetap bisa tampil juga"*.

**Apakah ini menyimpang dari desain?**  
Ini merupakan kombinasi `RULE_VIOLATION` terhadap aturan pengguna terkini dan `DESIGN_CHANGE` untuk melengkapi spesifikasi denah interaktif modul perinatologi rawat inap.

**Dampak nyata.**  
Jika kamar dewasa bisa diklik, petugas berisiko memilih bed dewasa untuk pasien bayi baru lahir, yang nantinya akan ditolak oleh backend dengan galat `NEWBORN_BED_REQUIRED` (*"Pasien bayi hanya dapat memakai tempat yang ditetapkan untuk bayi"*) saat tombol simpan ditekan. Sebaliknya jika ruangan lain dihilangkan, petugas kehilangan konteks navigasi denah lantai gedung rumah sakit.

**Rekomendasi.**  
Terapkan adaptor data denah (`adaptedFloorPlanData`) untuk mode `isNewbornAdmission`:
1. Seluruh kamar dan bed pada lantai yang dipilih tetap ditampilkan secara visual pada denah (*rendering full layout intact*).
2. Untuk mode **Neonatal Terpisah**:
   - Kamar ditandai `isSelectable = true` (garis batas hijau interaktif) **hanya jika** kamar tersebut memiliki bed khusus bayi (`isForNewborn === true`) yang memenuhi kriteria intensif (jika `requiresIntensiveCare` aktif).
   - Kamar non-bayi tetap ditampilkan dengan warna netral/abu-abu dan atribut `isSelectable = false` (tidak dapat diklik). Jika modal kamar dibuka, bed non-bayi menampilkan badge *"Khusus Pasien Dewasa / Bukan Bed Bayi"* dan tombol pilih dinonaktifkan.

---

### ISS-EPS-030-03 — Ketiadaan Validasi Guard Saat Kamar Penempatan Ibu Berstatus Closed pada Mode Rawat Gabung

| Field | Nilai |
| :--- | :--- |
| **No. laporan** | 3 |
| **Jenis** | `BUG` / `RULE_VIOLATION` |
| **Area** | Frontend (`use-inpatient-newborn-admission.js`, `NewbornCareStep`) & Backend (`InpNewbornAdmissionRules.cs`) |
| **Keparahan** | High |
| **Status bukti** | `SUDAH-VERIFIKASI` di source FE dan BE |
| **Perbaikan** | `FIX-EPS-030-03` |

**Apa yang terjadi.**  
Pengguna menetapkan aturan klinis:  
*"jika bersama ibu berarti pada kamar ibu tersebut , pastikan jika status ibu ya sudah closed kamar maka tidak bisa di pilih"*.  
Pada sistem saat ini, bila seorang ibu yang melahirkan sudah dipulangkan (*discharged*), penempatannya sudah diakhiri (*closed / EndDateTime != null*), atau kamar perawatannya ditutup (*bed/room closed*), petugas admisi di Langkah 3 masih dapat memilih opsi *"Rawat gabung dengan ibu"*. Sistem tidak memberikan peringatan bahwa kamar ibu sudah tutup, sehingga ketika petugas masuk ke langkah penempatan, sistem mengalami kebuntuan atau gagal validasi tanpa penjelasan yang ramah bagi staf medis.

**Kenapa terjadi.**  
1. Di backend `Areas/HealthServices/InPatientManagement/Services/InpEpisodeService.NewbornCandidates.cs:130-143`, query penempatan ibu mencari penempatan aktif:
   ```csharp
   var placementRows = await _dbContext.Set<InpBedPlacement>()
       .AsNoTracking()
       .Where(x => episodeIds.Contains(x.EpisodeId) && x.EndDateTime == null && !x.IsDelete)
   ```
   Bila penempatan ibu sudah selesai (`EndDateTime != null`), maka variabel `placement` bernilai `null`. Akibatnya DTO kandidat ibu mengembalikan `CurrentRoomId = null` dan `CurrentRoomName = null` (`lines 192-193`).
2. Namun, di frontend `src/lib/hooks/health-services/inpatient-management/use-inpatient-newborn-admission.js:192-200`:
   ```javascript
   const validateCare = useCallback((doctorId) => {
     const errors = validateNewbornCare(draft, flow.motherEpisodeId, doctorId);
     if (draft.careMode === "RoomingIn" && !selectedCandidate?.motherEpisodeId) {
       errors.motherEpisodeId = "Rawat gabung memerlukan episode ibu yang masih aktif.";
     }
     setError(Object.values(errors)[0] || "");
     return Object.keys(errors).length === 0;
   }, [draft, flow.motherEpisodeId, selectedCandidate]);
   ```
   Fungsi `validateCare` **hanya memeriksa keberadaan `motherEpisodeId`**, tanpa memeriksa apakah `selectedCandidate.currentRoomId` ada atau apakah kamar ibu masih berstatus aktif!
3. Jika ibu memiliki episode rawat inap tetapi penempatan kamarnya sudah selesai (*closed*), validasi Langkah 3 lolos dengan sukses.
4. Ketika sampai di backend pada evaluasi penempatan (`InpNewbornAdmissionRules.cs:70-81` dan `83-101`):
   Fungsi `FindMotherRoomAsync` mengembalikan `null`, sehingga `ValidateBed` menolak seluruh bed dengan galat `NEWBORN_ROOMING_IN_MISMATCH` (*"Rawat gabung memerlukan boks bayi di kamar penempatan ibu yang masih aktif"*). Papan ketersediaan bed menjadi kosong melompong (0 bed), yang kemudian memicu *crash* `prepareFloorPlanDraft` pada `ISS-EPS-030-01`.

**Apakah ini menyimpang dari desain?**  
Ya, ini adalah `BUG` dan pelanggaran aturan integritas relasi klinis rawat gabung. Pasien bayi tidak mungkin dirawat gabung di kamar ibu apabila ibu sudah tidak memiliki kamar aktif di rumah sakit.

**Dampak nyata.**  
Petugas admisi memproses pendaftaran bayi dengan status rawat gabung untuk ibu yang sebenarnya sudah di-discharge atau kamarnya sudah ditutup. Hal ini menyebabkan kebingungan penagihan biaya kamar (*billing bed rate mismatch*) dan kekacauan administrasi rekam medis rawat inap.

**Rekomendasi.**  
1. **Frontend Guard (Langkah 3)**: Tambahkan validasi eksplisit pada `useInpatientNewbornAdmission.js` (`validateCare`). Jika `careMode === "RoomingIn"`, periksa apakah kandidat ibu memiliki `currentRoomId` yang valid dan aktif. Bila kamar ibu sudah tidak aktif/closed, kunci opsi rawat gabung atau tampilkan pesan galat: *"Kamar penempatan ibu sudah berstatus tertutup (closed/selesai). Bayi tidak dapat didaftarkan rawat gabung; silakan pilih opsi Neonatal Terpisah atau periksa penempatan ibu."*
2. **Frontend Denah Guard (Langkah 4)**: Pada tampilan denah rawat gabung, sorot secara otomatis kamar ibu. Jika kamar penempatan ibu berstatus tertutup/closed, tandai kamar tersebut `isSelectable = false` dan tampilkan kotak informasi peringatan bahwa kamar ibu telah closed sehingga boks bayi di kamar tersebut tidak dapat dipesan.

---

### ISS-EPS-030-T1 — Kontrak Endpoint Denah Backend Mewajibkan `EpisodeId` Aktif Sehingga Menghambat Pemuatan Denah Pra-Admisi

| Field | Nilai |
| :--- | :--- |
| **No. laporan** | — (Temuan Tambahan Penelusuran) |
| **Jenis** | `CONTRACT_MISMATCH` |
| **Area** | Backend (`InpFloorPlanQueryService.cs`) |
| **Keparahan** | Medium |
| **Status bukti** | `SUDAH-VERIFIKASI` di source code backend |
| **Perbaikan** | `FIX-EPS-030-04` |
| **Kenapa dicantumkan** | Merupakan penyebab mengapa frontend terpaksa melakukan trik `prepareFloorPlanDraft` (membuat episode draf sebelum memilih bed). |

**Apa yang terjadi.**  
Endpoint backend `GET /bed-occupancies/floor-plan` pada berkas `Areas/HealthServices/InPatientManagement/Services/InpFloorPlanQueryService.cs:73-74` memiliki validasi ketat:
```csharp
if (query!.EpisodeId is not { } episodeId || episodeId == Guid.Empty)
    throw new InpBedOperationException(400, "INVALID_INPUT", "Episode wajib dikirim dari konteks layar.");
```
Validasi ini memaksa pemanggil mengirimkan `EpisodeId` yang valid di database. Akibatnya, alur yang belum memiliki episode di Langkah Penempatan (seperti Admisi Transfer dan Admisi Bayi) tidak dapat memanfaatkan endpoint ini secara langsung dan terpaksa melakukan pemanggilan ganda atau membuat episode sementara (*dummy*).

**Rekomendasi.**  
Frontend menggunakan endpoint layout master data `GET /mst-rooms/floor-plan` (`bedManagementService.getFloorPlan`) yang murni mengembalikan denah fisik lantai rumah sakit tanpa kewajiban episode, lalu memanfaatkan `legacyBoard.availableBeds` yang sudah membawa konteks kelayakan bayi untuk menghitung `isSelectable` secara deterministik di memori frontend.

---

## 5. Tanya-Jawab Pelapor

> **T:** Mengapa pada saat pendaftaran admisi bayi denah bed tersebut belum bisa muncul sama sekali?  
> **J:** Denah belum muncul karena sistem frontend mencoba membuat draf admisi di server secara otomatis sebelum denah ditampilkan. Pembuatan draf tersebut membutuhkan satu tempat tidur awal sebagai pancingan. Karena pada admisi bayi tempat tidur awal tersebut belum tersedia, sistem gagal dan memunculkan galat merah *"Episode Draft belum dapat disiapkan karena tidak ada konteks kamar aktif"*, yang akhirnya menyembunyikan denah seutuhnya.

> **T:** Bagaimana aturan tampilan kamar jika memilih opsi neonatal terpisah?  
> **J:** Sesuai permintaan Anda, seluruh denah lantai rumah sakit beserta kamar-kamar lainnya akan **tetap tampil utuh** agar petugas memiliki panduan denah gedung yang jelas. Namun, sistem akan mengunci selektivitasnya sehingga **hanya kamar bayi (perinatologi/NICU) dan tempat tidur khusus bayi (`isForNewborn = true`) saja yang dapat diklik dan dipilih**. Kamar umum/dewasa lainnya akan berwarna netral dan tidak bisa diklik.

> **T:** Bagaimana jika memilih opsi bersama ibu (rawat gabung), tetapi status kamar ibunya sudah closed?  
> **J:** Jika status penempatan kamar ibu sudah *closed* (ibu sudah pulang, dipindahkan, atau kamarnya selesai dirawat), maka kamar ibu tersebut **tidak dapat dipilih**. Sistem akan mendeteksi status ini sejak Langkah 3 dan Langkah 4, serta menampilkan peringatan bahwa kamar ibu telah tertutup sehingga petugas diarahkan untuk memilih opsi Neonatal Terpisah atau mengonfirmasi status kamar ibu ke petugas ruangan.

---

## 6. Temuan Tambahan

Temuan tambahan dicatat pada tabel bagian 3 di atas dengan kode `ISS-EPS-030-T1`:
- **Akar masalah ketergantungan `EpisodeId`**: Frontend terpaksa membuat episode draf prematur karena endpoint denah admisi backend `GET /bed-occupancies/floor-plan` menolak permintaan tanpa `EpisodeId` dengan galat 400. Solusi terbaik yang tidak mengubah kontrak backend adalah menggunakan denah layout master data umum lalu mengadaptasi selektivitasnya di frontend berbasis `available-beds`.

---

## 7. Pertanyaan dan Keputusan yang Dibutuhkan

### Untuk Pelapor
*Tidak ada pertanyaan yang menahan perbaikan. Seluruh instruksi dan aturan bisnis dari pelapor (tampilkan bed, hanya kamar bayi yang bisa diklik untuk neonatal terpisah, dan kunci kamar ibu jika sudah closed) sudah sangat jelas dan berbukti.*

### Untuk Pemilik Sistem

| No | Keputusan | Pilihan | Rekomendasi | Menahan Perbaikan |
| :---: | :--- | :--- | :--- | :--- |
| **K-01** | Penanganan bila ibu persalinan belum memiliki kamar aktif saat petugas memilih "Rawat gabung dengan ibu" di Langkah 3 | **A**: Blokir di Langkah 3 dengan pesan peringatan jelas.<br>**B**: Izinkan lanjut ke Langkah 4, lalu tandai kamar ibu tidak dapat dipilih di denah. | **Pilihan A & B terpadu**: Berikan peringatan validasi di Langkah 3 sekaligus tandai tidak dapat dipilih pada denah Langkah 4. | `FIX-EPS-030-03` |
| **K-02** | Penanda visual kamar non-bayi pada denah mode Neonatal Terpisah | **A**: Kamar non-bayi berwarna abu-abu netral, kursor tanda larangan (*not-allowed*), tidak membuka modal.<br>**B**: Kamar non-bayi dapat dibuka modalnya tetapi tombol pilih bed di-disable. | **Pilihan A**: Lebih cepat dan mencegah klik sia-sia oleh petugas. | `FIX-EPS-030-02` |

---

## 8. Catatan Pola

1. **Pola Kegagalan Zombie Draft & Dummy Preparation**: Masalah ini memiliki pola akar yang serupa dengan `ISSUE-EPS-024` (Admisi Transfer tanpa denah) dan `ISSUE-EPS-028` (Admisi Bayi melompat ke step 4). Pendekatan membuat draf episode sebelum tempat tidur benar-benar dipilih oleh pengguna (*speculative draft preparation*) selalu menimbulkan kerapuhan jika data prasyarat (*context bed*) belum lengkap.
2. **Pencegahan**: Standarisasi alur penempatan tempat tidur harus memisahkan secara tegas antara **Mode Eksplorasi/Pemilihan Denah** (*Read-Only Floor Plan Inspection*) dan **Titik Tulis Komitmen** (*Commit/Reservation Step*). Denah harus dapat dilihat tanpa perlu membuat entitas transaksi di database terlebih dahulu.

---

## 9. Riwayat Dokumen

| Tanggal | Perubahan | Oleh |
| :--- | :--- | :--- |
| 2026-10-21 | Dokumen issue dibuat berdasarkan laporan pengguna mengenai denah bed admisi bayi, aturan kamar bayi neonatal terpisah, dan penjagaan kamar ibu berstatus closed. | `diagnose-module-issue` |
| 2026-10-21 | Perbaikan FIX-EPS-030 selesai diimplementasikan pada frontend dan seluruh pengujian otomatis lulus. Issue ditandai SELESAI. | Tim Rekayasa Frontend |
