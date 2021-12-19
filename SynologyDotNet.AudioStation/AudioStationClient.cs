﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SynologyDotNet.AudioStation.Model;
using SynologyDotNet.Core.Helpers;
using SynologyDotNet.Core.Model;
using SynologyDotNet.Core.Responses;

namespace SynologyDotNet.AudioStation
{
    /// <summary>
    /// Connects to AudioStation APIs
    /// </summary>
    public sealed class AudioStationClient : StationConnectorBase
    {
        #region Fields

        /// <summary>
        /// This exists only if the AudioStation package has been installed.
        /// This endpoint is used to edit song metadata prgorammatically.
        /// </summary>
        private const string TagEditorEndpoint = "webman/3rdparty/AudioStation/tagEditorUI/tag_editor.cgi";

        #endregion Fields

        #region Apis

        const string SYNO_AudioStation_Info = "SYNO.AudioStation.Info";
        const string SYNO_AudioStation_Album = "SYNO.AudioStation.Album";
        const string SYNO_AudioStation_Composer = "SYNO.AudioStation.Composer";
        const string SYNO_AudioStation_Genre = "SYNO.AudioStation.Genre";
        const string SYNO_AudioStation_Artist = "SYNO.AudioStation.Artist";
        const string SYNO_AudioStation_Folder = "SYNO.AudioStation.Folder";
        const string SYNO_AudioStation_Song = "SYNO.AudioStation.Song";
        const string SYNO_AudioStation_Cover = "SYNO.AudioStation.Cover";
        const string SYNO_AudioStation_Stream = "SYNO.AudioStation.Stream";
        const string SYNO_AudioStation_Search = "SYNO.AudioStation.Search";
        const string SYNO_AudioStation_Lyrics = "SYNO.AudioStation.Lyrics";
        const string SYNO_AudioStation_Playlist = "SYNO.AudioStation.Playlist";

        protected override string[] GetImplementedApiNames() => new string[]
        {
            SYNO_AudioStation_Info,
            SYNO_AudioStation_Album,
            SYNO_AudioStation_Composer,
            SYNO_AudioStation_Genre,
            SYNO_AudioStation_Artist,
            SYNO_AudioStation_Folder,
            SYNO_AudioStation_Song,
            SYNO_AudioStation_Cover,
            SYNO_AudioStation_Stream,
            SYNO_AudioStation_Search,
            SYNO_AudioStation_Lyrics,
            SYNO_AudioStation_Playlist
        };

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether only the personal music folder is used.
        /// True if only the current user's personal music folder is used. 
        /// False if the personal and the shared music are used.
        /// </summary>
        /// <value>
        ///   <c>true</c> if only the current user's personal music folder is used; <c>false</c> if both the personal and the shared music are used.
        /// </value>
        public bool PersonalMusicOnly { get; set; }

        #endregion Properties

        /// <summary>Initializes a new instance of the <see cref="AudioStationClient" /> class.</summary>
        public AudioStationClient() : base()
        {
        }

        #region Folder

        /// <summary>
        /// List folders
        /// </summary>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="offset">Start position in the list (use it for paging)</param>
        /// <param name="folderId">Root folder ID, the children directories will be listed in the response. Not recursive.</param>
        /// <returns></returns>
        public async Task<ApiListRessponse<FolderList>> ListFoldersAsync(int limit, int offset, string folderId = null)
        {
            var args = new List<(string, object)>();
            args.Add(GetLibraryArg());
            if (!string.IsNullOrEmpty(folderId))
                args.Add(("id", folderId));

            return await Client.QueryListAsync<ApiListRessponse<FolderList>>(SYNO_AudioStation_Folder, "list", limit, offset, args.ToArray());
        }

        #endregion Folder

        #region Artist

        /// <summary>
        /// List artists
        /// </summary>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="offset">Start position in the list (use it for paging)</param>
        /// <returns></returns>
        public async Task<ApiListRessponse<ArtistList>> ListArtistsAsync(int limit, int offset)
        {
            return await Client.QueryListAsync<ApiListRessponse<ArtistList>>(SYNO_AudioStation_Artist, "list", limit, offset, GetLibraryArg()); //personal
        }

        /// <summary>
        /// Download artist cover image
        /// </summary>
        /// <param name="artist">Artist name</param>
        /// <returns></returns>
        public async Task<ByteArrayData> GetArtistCoverAsync(string artist)
        {
            return await Client.QueryByteArrayAsync(SYNO_AudioStation_Cover, "getcover",
                GetLibraryArg(),
                ("artist_name", artist));
        }

        #endregion Artist

        #region Album

        /// <summary>
        /// List albums
        /// </summary>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="offset">Start position in the list (use it for paging)</param>
        /// <param name="artist">Filter by artist name</param>
        /// <param name="queryParameters">Filter parameters</param>
        /// <returns></returns>
        public async Task<ApiListRessponse<AlbumList>> ListAlbumsAsync(int limit, int offset, string artist = null, params (AlbumQueryParameters, object)[] queryParameters)
        {
            var args = new List<(string, object)>(queryParameters.Select(f => (f.Item1.ToString(), f.Item2)));
            args.Add(GetLibraryArg());
            if (!string.IsNullOrWhiteSpace(artist))
                args.Add(("artist", artist));

            return await Client.QueryListAsync<ApiListRessponse<AlbumList>>(SYNO_AudioStation_Album, "list", limit, offset, args.ToArray());
        }

        /// <summary>
        /// Download album cover image
        /// </summary>
        /// <param name="artist">Artist name</param>
        /// <param name="album">Album title</param>
        /// <returns></returns>
        public async Task<ByteArrayData> GetAlbumCoverAsync(string artist, string album)
        {
            return await Client.QueryByteArrayAsync(SYNO_AudioStation_Cover, "getcover",
                ("album_name", album),
                ("album_artist_name", artist));
        }

        #endregion Album

        #region Song

        /// <summary>
        /// List songs
        /// </summary>
        /// <param name="limit">Maximum number of items to return</param>
        /// <param name="offset">Start position in the list (use it for paging)</param>
        /// <param name="additional">Additional filds to load</param>
        /// <param name="queryParameters">Filter parameters</param>
        /// <returns></returns>
        public async Task<ApiListRessponse<SongList>> ListSongsAsync(int limit, int offset, SongQueryAdditional additional, params (SongQueryParameters, object)[] queryParameters)
        {
            var args = new List<(string, object)>(queryParameters.Select(f => (f.Item1.ToString(), f.Item2)));
            args.Add(GetLibraryArg());
            if (additional != SongQueryAdditional.None)
            {
                args.Add(("additional", string.Join(",", (new[] {
                        SongQueryAdditional.song_audio,
                        SongQueryAdditional.song_rating,
                        SongQueryAdditional.song_tag
                    })
                    .Where(x => additional.HasFlag(x))
                    .Select(x => x.ToString()))));
            }
            var result = await Client.QueryListAsync<ApiListRessponse<SongList>>(SYNO_AudioStation_Song, "list", limit, offset, args.ToArray());
            return result;
        }

        /// <summary>
        /// Gets a song by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ApiListRessponse<SongList>> GetSongByIdAsync(string id)
        {
            var args = new List<(string, object)>();
            args.Add(("id", id));
            args.Add(("additional", "song_tag, song_audio, song_rating")); // request detailed song info
            var result = await Client.QueryObjectAsync<ApiListRessponse<SongList>>(SYNO_AudioStation_Song, "getinfo", args.ToArray());
            return result;
        }

        /// <summary>
        /// Set song rating
        /// </summary>
        /// <param name="songId"></param>
        /// <param name="rating">Value from 0 to 5</param>
        /// <returns></returns>
        public async Task<ApiResponse> RateSongAsync(string songId, int rating)
        {
            if (rating < 0 || rating > 5)
                throw new ArgumentOutOfRangeException(nameof(rating), "Value range: 0 - 5");
            var result = await Client.QueryObjectAsync<ApiResponse>(SYNO_AudioStation_Song, "setrating",
                ("id", songId),
                ("rating", rating));
            return result;
        }

        /// <summary>
        /// Download the song from the server.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="transcode">Transcode method</param>
        /// <param name="songId">ID of the song to download</param>
        /// <param name="positionSeconds">Start position in seconds</param>
        /// <param name="readStreamAction">The action called to download the song bytes.</param>
        /// <returns></returns>
        public async Task StreamSongAsync(
            CancellationToken cancellationToken,
            TranscodeMode transcode,
            string songId,
            double positionSeconds,
            Action<StreamResult> readStreamAction)
        {
            var req = CreateSongStreamRequest(SYNO_AudioStation_Stream, transcode, songId, positionSeconds);
            await Client.QueryStreamAsync(req, readStreamAction, cancellationToken);
        }

        #endregion

        // Use tageditor instead!
        //#region Lyrics
        //public async Task<ApiDataResponse<Lyrics>> GetLyricsAsync(string songId)
        //{
        //	var result = await Client.QueryAsync<ApiDataResponse<Lyrics>>(Syno_AudioStation_Lyrics, "getlyrics", ("id", songId));
        //	return result;
        //}

        //public async Task<ApiResponse> SetLyricsAsync(string songId, string lyrics)
        //{
        //	var result = await Client.QueryAsync<ApiResponse>(Syno_AudioStation_Lyrics, "setlyrics",
        //		("id", songId),
        //		("lyrics", lyrics));
        //	return result;
        //}
        //#endregion

        #region Tags

        /// <summary>
        /// Query song tags
        /// </summary>
        /// <param name="paths">The internal path of the music file. Must contain forward slaashes '/'</param>
        /// <returns></returns>
        public async Task<FileTags> GetSongFileTags(params string[] paths)
        {
            if (paths?.Any() != true)
                throw new ArgumentNullException(nameof(paths));
            if (paths.Any(p => p.Contains("\\")))
                throw new ArgumentException("Invalid path. Path must contain forward slashes '/', not back-slashes '\\'");
            var req = new RequestBuilder().SetEndpoint(TagEditorEndpoint).Action("load");
            req["audioInfos"] = JsonConvert.SerializeObject(paths.Select(p => new { path = p }));
            req["requestFrom"] = string.Empty;
            var result = await Client.QueryObjectAsync<FileTags>(req);
            return result;
        }

        /// <summary>
        /// Batch edit music file tags
        /// </summary>
        /// <param name="change"></param>
        /// <returns></returns>
        public async Task<ApiResponse> SetSongFileTags(FileTagChange change)
        {
            if (change?.AudioInfos?.Any() != true)
                throw new ArgumentNullException($"{nameof(change)}.{nameof(change.AudioInfos)}");

            var req = new RequestBuilder().SetEndpoint(TagEditorEndpoint).Action("apply");
            req["data"] = JsonConvert.SerializeObject(new object[] { change });
            var result = await Client.QueryObjectAsync<ApiResponse>(req);
            return result;
        }

        #endregion

        #region Search        

        /// <summary>
        /// Searches the music library.
        /// </summary>
        /// <param name="keyword">The text to search.</param>
        /// <returns></returns>
        public async Task<ApiDataResponse<SearchResults>> SearchAsync(string keyword)
        {
            var args = new List<(string, object)>();
            args.Add(("additional", "song_tag, song_audio, song_rating")); // request detailed song info
            args.Add(("keyword", keyword));
            var result = await Client.QueryObjectAsync<ApiDataResponse<SearchResults>>(SYNO_AudioStation_Search, "list", args.ToArray());
            return result;
        }

        #endregion

        #region Playlist
        public async Task<ApiListRessponse<PlaylistList>> ListPlaylistsAsync(int limit, int offset)
        {
            var result = await Client.QueryListAsync<ApiListRessponse<PlaylistList>>(SYNO_AudioStation_Playlist, "list", limit, offset,
                ("library", "all"));
            return result;
        }

        public async Task<ApiDataResponse<Playlist>> GetPlaylistAsync(int limit, int offset, string id)
        {
            var result = await Client.QueryListAsync<ApiListRessponse<PlaylistList>>(SYNO_AudioStation_Playlist, "getinfo", limit, offset,
                ("library", "all"),
                ("id", id),
                ("additional", "songs") //("additional", "songs_song_tag,songs_song_audio,songs_song_rating,sharing_info")
            );
            return new ApiDataResponse<Playlist>(result, result.Data?.playlists?.FirstOrDefault() ?? default);
        }
        #endregion

        #region Private Methods

        private (string, string) GetLibraryArg() => ("library", PersonalMusicOnly ? "personal" : "all");

        private RequestBuilder CreateSongStreamRequest(string apiName, TranscodeMode transcode, string songId, double positionInSeconds)
        {
            string method = "stream";
            string subEndpoint = null;
            string format = null;
            switch (transcode)
            {
                case TranscodeMode.MP3_128Kbps:
                case TranscodeMode.MP3_192Kbps:
                case TranscodeMode.MP3_256Kbps:
                case TranscodeMode.MP3_320Kbps:
                    subEndpoint = "/0.mp3";
                    method = "transcode";
                    format = "mp3";
                    break;
                case TranscodeMode.WAV:
                    subEndpoint = "/0.wav";
                    method = "transcode";
                    format = "wav";
                    break;
            }
            var req = new RequestBuilder(Client.GetApiInfo(apiName), subEndpoint).Method(method).SetParam("id", songId);
            if (!string.IsNullOrEmpty(format))
                req.SetParam("format", format);
            if (positionInSeconds > 0)
                req.SetParam("position", Math.Round(positionInSeconds, 4).ToString(CultureInfo.InvariantCulture));
            switch (transcode)
            {
                case TranscodeMode.MP3_128Kbps:
                    req.SetParam("bitrate", "128000");
                    break;
                case TranscodeMode.MP3_192Kbps:
                    req.SetParam("bitrate", "192000");
                    break;
                case TranscodeMode.MP3_256Kbps:
                    req.SetParam("bitrate", "256000");
                    break;
                case TranscodeMode.MP3_320Kbps:
                    req.SetParam("bitrate", "320000");
                    break;
            }
            return req;
        }
        #endregion
    }
}